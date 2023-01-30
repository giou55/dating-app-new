using api.Extensions;
using api.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace api.SignalR
{
    public class MessageHub: Hub
    {
        private readonly IMessageRepository _messageRepository;

        public MessageHub(IMessageRepository messageRepository)
        {
            _messageRepository = messageRepository;
            
        }

        // we're going to override the two methods: OnConnectedAsync and OnDisconnectedAsync

        public override async Task OnConnectedAsync()
        {
            // we want the name of the user that we are connected to,
            // so we need to get access to the Http Context,
            // because when we do make a connection to a SignalR hub,
            // we do send up a HTTP request to initialize that connection,
            // that give us an opportunity to send something in a query string
            var httpContext = Context.GetHttpContext();
            var otherUser = httpContext.Request.Query["user"];

            // then we need to put the two users in a group
            var groupName = GetGroupName(Context.User.GetUsername(), otherUser);
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            // we get the message thread between him and the user he is connected to
            var messages = await _messageRepository.GetMessageThread(Context.User.GetUsername(), otherUser);

            // when a user connects to the Message hub,
            // he is going to receive the message thread from SignalR,
            // instead of making an API call from Angular component
            await Clients.Group(groupName).SendAsync("ReceiveMessageTread", messages);
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            // because we're passing an exception to this method,
            // we do need to call the base onDisconnected method
            return base.OnDisconnectedAsync(exception);
        }

        private string GetGroupName(string caller, string other)
        {
            // the group name is going to be a combination of two usernames,
            // we just need to sort these into alphabetical order,
            // so it doesn't matter which user connects first,
            // the group name is always going to be the same
            var stringCompare = string.CompareOrdinal(caller, other) < 0 ; // make this boolean
            return stringCompare ? $"{caller}-{other}" : $"{other}-{caller}";
        }        
    }
}