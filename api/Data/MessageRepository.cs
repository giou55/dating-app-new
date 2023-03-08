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

        public void AddGroup(Group group)
        {
            _context.Groups.Add(group);
        }

        public void AddMessage(Message message)
        {
            _context.Messages.Add(message);
        }

        public void DeleteMessage(Message message)
        {
            _context.Messages.Remove(message);
        }

        public async Task<Connection> GetConnection(string connectionId)
        {
            return await _context.Connections.FindAsync(connectionId);
        }

        public async Task<Group> GetGroupForConnection(string connectionId)
        {
            return await _context.Groups
                .Include(x => x.Connections)
                .Where(x => x.Connections.Any(c => c.ConnectionId == connectionId))
                .FirstOrDefaultAsync();
        }

        public async Task<Message> GetMessage(int id)
        {
            return await _context.Messages.FindAsync(id);
        }

        public async Task<Group> GetMessageGroup(string groupName)
        {
            return await _context.Groups
                .Include(x => x.Connections)
                .FirstOrDefaultAsync(x => x.Name == groupName);
        }

        public async Task<PagedList<MessageDto>> GetMessagesForUser(MessageParams messageParams)
        {
            var query = _context.Messages
                .OrderByDescending(x => x.MessageSent)
                .AsQueryable();

            query = messageParams.Container switch
            {
                "Inbox" => query.Where(
                    u => u.RecipientUsername == messageParams.Username && u.RecipientDeleted == false),
                "Outbox" => query.Where(
                    u => u.SenderUsername == messageParams.Username && u.SenderDeleted == false),
                // the default case is going to be the unread messages
                _ => query.Where(
                    u => u.RecipientUsername == messageParams.Username && u.RecipientDeleted == false
                        && u.DateRead == null)
            };

            var messages = query.ProjectTo<MessageDto>(_mapper.ConfigurationProvider);

            return await PagedList<MessageDto>
                .CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);
        }

        public async Task<IEnumerable<MessageDto>> GetMessageThread(string currentUserName, string recipientUserName)
        {
            // this is the old bad query that we don't use anymore
            var messages = await _context.Messages
                // if we're using projection, we don't need to load the other related entities,
                // so we can remove this two lines
                .Include(u => u.Sender).ThenInclude(p => p.Photos)
                .Include(u => u.Recipient).ThenInclude(p => p.Photos)
                .Where(
                    m => m.RecipientUsername == currentUserName
                        && m.RecipientDeleted == false
                        && m.SenderUsername == recipientUserName ||
                        m.RecipientUsername == recipientUserName
                        && m.SenderDeleted == false
                        && m.SenderUsername == currentUserName
                )
                .OrderBy(m => m.MessageSent)
                .ToListAsync();

            // this is the new better query
            var query = _context.Messages
                .Where(
                    m => m.RecipientUsername == currentUserName
                        && m.RecipientDeleted == false
                        && m.SenderUsername == recipientUserName ||
                        m.RecipientUsername == recipientUserName
                        && m.SenderDeleted == false
                        && m.SenderUsername == currentUserName
                )
                .OrderBy(m => m.MessageSent)
                .AsQueryable();

            // we replaced messages with query for the new query
            var unreadMessages = query
                .Where(m => m.DateRead == null && m.RecipientUsername == currentUserName)
                .ToList();

            if (unreadMessages.Any()) // check if we have any unread messages
            {
                foreach (var message in unreadMessages)
                {
                    message.DateRead = DateTime.UtcNow; // we update the DateRead with right now
                }
            }

            return await query.ProjectTo<MessageDto>(_mapper.ConfigurationProvider).ToListAsync();
        }

        public void RemoveConnection(Connection connection)
        {
            _context.Connections.Remove(connection);
        }
    }
}