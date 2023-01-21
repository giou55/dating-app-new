using api.DTOs;
using api.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using api.Extensions;
using api.Entities;
using api.Helpers;

namespace api.Controllers
{
    public class MessagesController : BaseApiController
    {
        private readonly IUserRepository _userRepository;
        private readonly IMessageRepository _messageRepository;
        private readonly IMapper _mapper;

        public MessagesController
        (
            IUserRepository userRepository,
            IMessageRepository messageRepository,
            IMapper mapper // we want to map from Message to MessageDto
        )
        {
            _userRepository = userRepository;
            _messageRepository = messageRepository;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto)
        {
            // we get username from claims principal extension
            var username = User.GetUsername();

            if (username == createMessageDto.RecipientUsername.ToLower())
                return BadRequest("You cannot send messages to yourself");

            var sender = await _userRepository.GetUserByUsernameAsync(username);
            var recipient = await _userRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);

            // recipient is a property we're receiving from the client, and we need a check
            if (recipient == null) return NotFound();

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
            if (await _messageRepository.SaveAllAsync()) return Ok(_mapper.Map<MessageDto>(message));

            return BadRequest("Failed to send message");
        }

        [HttpGet]
        public async Task<ActionResult<PagedList<MessageDto>>> GetMessagesForUser
            ([FromQuery]MessageParams messageParams)
        {
            // we get username from claims principal extension
            messageParams.Username = User.GetUsername();

            var messages = await _messageRepository.GetMessagesForUser(messageParams);

            Response.AddPaginationHeader(
                new PaginationHeader(
                    messages.CurrentPage, messages.PageSize, messages.TotalCount, messages.TotalPages
            ));

            return messages;
        }

        [HttpGet("thread/{username}")]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessageThread(string username)
        {
            // we get username from claims principal extension
            var currentUsername = User.GetUsername();

            return Ok(await _messageRepository.GetMessageThread(currentUsername, username));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMessage(int id)
        {
            // we get username from claims principal extension
            var username = User.GetUsername();

            var message = await _messageRepository.GetMessage(id);

            if (message.SenderUsername != username && message.RecipientUsername != username)
                return Unauthorized();

            if (message.SenderUsername == username) message.SenderDeleted = true;
            if (message.RecipientUsername == username) message.RecipientDeleted = true;

            // we're only deleting the message itself if both the sender and the
            // recipient have both decided they want to delete the message,
            // otherwise we're going to keep it in our database
            if (message.SenderDeleted && message.RecipientDeleted)
            {
                _messageRepository.DeleteMessage(message);
            }

            if (await _messageRepository.SaveAllAsync()) return Ok();
            
            return BadRequest("Problem deleting the message");
        }
    }
}