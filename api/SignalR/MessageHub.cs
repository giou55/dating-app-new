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

        public MessageHub(
            IMessageRepository messageRepository, 
            IUserRepository userRepository,
            IMapper mapper
        )
        {
            _messageRepository = messageRepository;
            _userRepository = userRepository;
            _mapper = mapper;
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

            // in order for Entity framework to track this, 
            // we need to use AddMessage to add the message
            _messageRepository.AddMessage(message);

            // after the message is saved, it'll be sent to the client to be displayed to the view
            if (await _messageRepository.SaveAllAsync()) 
            {
                //return Ok(_mapper.Map<MessageDto>(message));
                var group = GetGroupName(sender.UserName, recipient.UserName);
                await Clients.Group(group).SendAsync("NewMessage", _mapper.Map<MessageDto>(message));
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
    }
}