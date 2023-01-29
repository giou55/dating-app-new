using api.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace api.SignalR
{
    [Authorize] // only authorized users are able to access this hub
    public class PresenceHub : Hub
    {
        private readonly PresenceTracker _tracker;
        public PresenceHub(PresenceTracker tracker)
        {
            _tracker = tracker;
        }

        public override async Task OnConnectedAsync()
        {
            await _tracker.UserConnected(Context.User.GetUsername(), Context.ConnectionId);
            var currentUsers = await _tracker.GetOnlineUsers();

            // Clients object can be used to invoke methods on clients 
            // that are connected to this hub

            // we've got the option to send a message to all connected users,
            // or just to the user that's calling this method on the hub,
            // or send it to everyone else apart from the user that's just connected

            // here when this user does connect
            // anybody else that's connected to this same hub
            // is going to receive the username of the user that has just connected,
            // and Context give us access to our user claims principle
            await Clients.Others.SendAsync("UserIsOnline", Context.User.GetUsername());

            // we send this back to all of the connected users,
            // so this is going to allow clients connected to our application to 
            // update their list of who is currently online,
            // and we can display that information in the browser
            await Clients.All.SendAsync("GetOnlineUsers", currentUsers);
        }

        public override async  Task OnDisconnectedAsync(Exception exception)
        {
            await _tracker.UserDisconnected(Context.User.GetUsername(), Context.ConnectionId);

            // currentUsers contains online users except the user that have just been removed
            var currentUsers = await _tracker.GetOnlineUsers();

            // Clients object also can be used to invoke methods on clients 
            // that are disconnected to this hub

            // here when this user does disconnect
            // anybody else that's connected to this same hub
            // is going to receive the username of the user that has just disconnected,
            // and Context give us access to our user claims principle
            await Clients.Others.SendAsync("UserIsOffline", Context.User.GetUsername());

            // we send this back to all of the connected users,
            // so this is going to allow clients connected to our application to 
            // update their list of who is currently online,
            // and we can display that information in the browser
            await Clients.All.SendAsync("GetOnlineUsers", currentUsers);

            // because we're passing an exception to this method,
            // we do need to call the base onDisconnected method
            await base.OnDisconnectedAsync(exception);
        }
    }
}