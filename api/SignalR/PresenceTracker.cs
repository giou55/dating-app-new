namespace api.SignalR
{
    // we're going to store the connected users in memory on our server,
    // this approach is for a single server, and it's not scalable at all.

    // this class tracks who is currently connected to our hub
    public class PresenceTracker
    {
        // the first key represents the username of user,
        // the second key is a list of connection IDs for that user,
        // because there's nothing to stop the user from logging in again
        // from a different device, and it'll be given a different connection ID     
        private static readonly Dictionary<string, List<string>> OnlineUsers = 
            new Dictionary<string, List<string>>();
        
        // dictionary is not a thread safe type of object,
        // and if we have multiple concurrent users trying to access this 
        // dictionary at the same time, we could run into an issue
        public Task<bool> UserConnected(string username, string connectionId)
        {
            bool isOnLine = false;

            // we're going to lock our online users whilst we're adding 
            // the on connecting user to this dictionary,
            // and anyone else that's connecting, 
            // they'll have to wait their turn to be added to this
            lock(OnlineUsers) 
            {
                if (OnlineUsers.ContainsKey(username)) // if they are already connected
                {
                    // add connectionId to the list of connection Ids  
                    OnlineUsers[username].Add(connectionId);
                } 
                else // they are not already connected
                {
                   OnlineUsers.Add(username, new List<string>{connectionId}); 
                   isOnLine = true;
                }
            }

            //return Task.CompletedTask;
            return Task.FromResult(isOnLine);
        }

        public Task<bool> UserDisconnected(string username, string connectionId)
        {
            bool isOffLine = false;

            lock(OnlineUsers) 
            {
                // if username is not in our dictionary as a key,
                // then we've got nothing to remove
                 if (!OnlineUsers.ContainsKey(username)) return Task.FromResult(isOffLine);
                // else remove the connectionId 
                OnlineUsers[username].Remove(connectionId);

                // if that user has not any connectionId,
                // remove user from the dictionary
                if (OnlineUsers[username].Count == 0) 
                {
                    OnlineUsers.Remove(username);
                    isOffLine = true;
                }
            }

            //return Task.CompletedTask;
            return Task.FromResult(isOffLine);
        }

        public Task<string[]> GetOnlineUsers() 
        {
            string[] onlineUsers;

            lock(OnlineUsers) 
            {
                // we get an alphabetical list of the users that are online,
                // and we're only interested about the key which is the username
                onlineUsers = OnlineUsers.OrderBy(k => k.Key).Select(k => k.Key).ToArray();
            }

            // we pass in our list of online users
            return Task.FromResult(onlineUsers);
                
        }

        public static Task<List<string>> GetConnectionsForUser(string username)
        {
            List<string> connectionsIds;

            lock(OnlineUsers) 
            {
                // we're getting a list of the connections for that particular user
                connectionsIds = OnlineUsers.GetValueOrDefault(username);
            }

            return Task.FromResult(connectionsIds);
        } 
    }
}