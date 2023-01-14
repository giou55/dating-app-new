using api.DTOs;
using api.Entities;
using api.Helpers;
using api.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace api.Data
{
    public class MessageRepository : IMessageRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        public MessageRepository(DataContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;  
        }

        public void AddMessage(Message message)
        {
            _context.Messages.Add(message);
        }

        public void DeleteMessage(Message message)
        {
            _context.Messages.Remove(message);
        }

        public async Task<Message> GetMessage(int id)
        {
            return await _context.Messages.FindAsync(id);
        }

        public async Task<PagedList<MessageDto>> GetMessagesForUser(MessageParams messageParams)
        {
            var query = _context.Messages
                .OrderByDescending(x => x.MessageSent)
                .AsQueryable();

            query = messageParams.Container switch
            {
                "Inbox" => query.Where(u => u.RecipientUsername == messageParams.Username),
                "Outbox" => query.Where(u => u.SenderUsername == messageParams.Username),
                // the default case is going to be the unread messages
                _ => query.Where(u => u.RecipientUsername == messageParams.Username && u.DateRead == null)
            };

            var messages = query.ProjectTo<MessageDto>(_mapper.ConfigurationProvider);

            return await PagedList<MessageDto>
                .CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);
        }

        public async Task<IEnumerable<MessageDto>> GetMessageThread(string currentUserName, string recipientUserName)
        {
            var messages = await _context.Messages
                .Include(u => u.Sender).ThenInclude(p => p.Photos)
                .Include(u => u.Recipient).ThenInclude(p => p.Photos)
                .Where(
                    m => m.RecipientUsername == currentUserName && m.SenderUsername == recipientUserName || 
                        m.RecipientUsername == recipientUserName && m.SenderUsername == currentUserName
                )
                .OrderBy(m => m.MessageSent)
                .ToListAsync();
            
            // get from memory only unread messages that current user has received
            var unreadMessages = messages
                .Where(m => m.DateRead == null && m.RecipientUsername == currentUserName)
                .ToList();

            if (unreadMessages.Any()) // check if we have any unread messages
            {
                foreach (var message in unreadMessages)
                {
                    message.DateRead = DateTime.UtcNow; // we update the DateTime
                }

                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> SaveAllAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}