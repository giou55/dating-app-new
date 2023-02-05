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
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;

        public MessagesController
        (
            IMapper mapper, // we want to map from Message to MessageDto
            IUnitOfWork uow
        )
        {
            _uow = uow;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto)
        {
            // we have copied all this code inside this method, 
            // and paste it at SendMessage method inside MessageHub class

            // we get username from claims principal extension
            var username = User.GetUsername();

            if (username == createMessageDto.RecipientUsername.ToLower())
                return BadRequest("You cannot send messages to yourself");

            var sender = await _uow.UserRepository.GetUserByUsernameAsync(username);
            var recipient = await _uow.UserRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);

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
            _uow.MessageRepository.AddMessage(message);

            // after the message is saved, it'll be sent to the client to be displayed to the view
            if (await _uow.Complete()) return Ok(_mapper.Map<MessageDto>(message));

            return BadRequest("Failed to send message");
        }

        [HttpGet]
        public async Task<ActionResult<PagedList<MessageDto>>> GetMessagesForUser
            ([FromQuery]MessageParams messageParams)
        {
            // we get username from claims principal extension
            messageParams.Username = User.GetUsername();

            var messages = await _uow.MessageRepository.GetMessagesForUser(messageParams);

            Response.AddPaginationHeader(
                new PaginationHeader(
                    messages.CurrentPage, messages.PageSize, messages.TotalCount, messages.TotalPages
            ));

            return messages;
        }

        // we don't use this endpoint anymore to get the Message Thread,
        // now we're using SignalR to get the Message Thread
        [HttpGet("thread/{username}")] 
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessageThread(string username)
        {
            // we get username from claims principal extension
            var currentUsername = User.GetUsername();

            return Ok(await _uow.MessageRepository.GetMessageThread(currentUsername, username));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMessage(int id)
        {
            // we get username from claims principal extension
            var username = User.GetUsername();

            var message = await _uow.MessageRepository.GetMessage(id);

            if (message.SenderUsername != username && message.RecipientUsername != username)
                return Unauthorized();

            if (message.SenderUsername == username) message.SenderDeleted = true;
            if (message.RecipientUsername == username) message.RecipientDeleted = true;

            // we're only deleting the message itself if both the sender and the
            // recipient have both decided they want to delete the message,
            // otherwise we're going to keep it in our database
            if (message.SenderDeleted && message.RecipientDeleted)
            {
                _uow.MessageRepository.DeleteMessage(message);
            }

            if (await _uow.Complete()) return Ok();
            
            return BadRequest("Problem deleting the message");
        }
    }
}