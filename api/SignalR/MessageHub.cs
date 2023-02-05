using api.DTOs;
using api.Entities;
using api.Extensions;
using api.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace api.SignalR
{
    [Authorize] // only authorized users are able to access this hub
    public class MessageHub: Hub
    {
        private readonly IMessageRepository _messageRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IHubContext<PresenceHub> _presenceHub;

        public MessageHub(
            IMessageRepository messageRepository, 
            IUserRepository userRepository,
            IMapper mapper,
            IHubContext<PresenceHub> presenceHub // this way we can access other hubs
        )
        {
            _messageRepository = messageRepository;
            _userRepository = userRepository;
            _mapper = mapper;
            _presenceHub = presenceHub;
        }

        // we're going to override the two methods: OnConnectedAsync and OnDisconnectedAsync

        // the user connects to the Message hub, when click the messages tab
        // and disconnects if go anywhere else 

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
            await AddToGroup(groupName);

            // we get the message thread between him and the user he is connected to
            var messages = await _messageRepository.GetMessageThread(Context.User.GetUsername(), otherUser);

            // when a user connects to the Message hub,
            // he is going to receive the message thread from SignalR,
            // instead of making an API call from Angular component
            await Clients.Group(groupName).SendAsync("ReceiveMessageTread", messages);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await RemoveFromMessageGroup();

            // because we're passing an exception to this method,
            // we do need to call the base onDisconnected method
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(CreateMessageDto createMessageDto)
        {
            // we get username from Hub.Context
            var username = Context.User.GetUsername();

            if (username == createMessageDto.RecipientUsername.ToLower())
                // because we're not inside an API controller,
                // we cannot return HTTP responses, so we throw exceptions,
                // exceptions cost more resources on our server, than a simple HTTP response

                // return BadRequest("You cannot send messages to yourself");
                throw new HubException("You cannot send messages to yourself");


            var sender = await _userRepository.GetUserByUsernameAsync(username);
            var recipient = await _userRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);

            // recipient is a property we're receiving from the client, and we need a check
            if (recipient == null) 
            // return NotFound();
            throw new HubException("Not found user");

            var message = new Message 
            {
                Sender = sender,
                Recipient = recipient,
                SenderUsername = sender.UserName,
                RecipientUsername = recipient.UserName,
                Content = createMessageDto.Content
            };

            var groupName = GetGroupName(sender.UserName, recipient.UserName);

            // now that we have the groupName, we can get the group from database 
            var group = await _messageRepository.GetMessageGroup(groupName);

            // and now we can check our connections and see if we do have a username
            // inside there that matches the recipient username,
            // and if so, it means recipient is inside the same message group
            // and we can mark the message as read
            if (group.Connections.Any(x => x.Username == recipient.UserName))
            {
                message.DateRead = DateTime.UtcNow;
            }
            else
            {
                var connections = await PresenceTracker.GetConnectionsForUser(recipient.UserName);
                if (connections != null) // then we know that the user is connected to our application 
                {
                    // we're sending a message to all of the clients connected from that user 
                    await _presenceHub.Clients.Clients(connections)
                        .SendAsync("NewMessageReceived", 
                            new {username = sender.UserName, knownAs = sender.KnownAs});
                }
            }

            // in order for Entity framework to track this, 
            // we need to use AddMessage to add the message
            _messageRepository.AddMessage(message);

            // after the message is saved, it'll be sent to the client to be displayed to the view
            if (await _messageRepository.SaveAllAsync()) 
            {
                //return Ok(_mapper.Map<MessageDto>(message));
                await Clients.Group(groupName).SendAsync("NewMessage", _mapper.Map<MessageDto>(message));
            }

            //return BadRequest("Failed to send message");
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

        // this is called inside OnConnectedAsync method,
        // when the user connects to the Message hub
        private async Task<bool> AddToGroup(string groupName) 
        {
            var group = await _messageRepository.GetMessageGroup(groupName);
            var connection = new Connection(Context.ConnectionId, Context.User.GetUsername());

            if (group == null)
            {
                group = new Group(groupName);
                _messageRepository.AddGroup(group);
            }

            group.Connections.Add(connection);

            return await _messageRepository.SaveAllAsync(); // this returns a boolean
        }  

        // this is called inside OnDisconnectedAsync method,
        // when the user disconnects to the Message hub

        // here we're removing the connection from Connections table in our database,
        // and we're not actually removing this from the message group in SignalR,
        // because when we do disconnect from SignalR inside OnDisconnectedAsync method,
        // then SignalR is automatically removing this connection from the SignalR group  
        private async Task RemoveFromMessageGroup()
        {
            var connection = await _messageRepository.GetConnection(Context.ConnectionId);
            _messageRepository.RemoveConnection(connection);
            await _messageRepository.SaveAllAsync();
        }
    }
}