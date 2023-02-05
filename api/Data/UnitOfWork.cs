using api.Interfaces;
using AutoMapper;

namespace api.Data
{
    // we're going to be injecting this into our controllers
    // instead of the repositories

    // when this is created, it's going to create a new instance of anything
    // we're using inside that class
    public class UnitOfWork : IUnitOfWork
    {
        private readonly IMapper _mapper;
        private readonly DataContext _context;
        public UnitOfWork(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public IUserRepository UserRepository => new UserRepository(_context, _mapper);

        public IMessageRepository MessageRepository => new MessageRepository(_context, _mapper);

        public ILikesRepository LikesRepository => new LikesRepository(_context);

        public async Task<bool> Complete()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        // we want to know if the context has changes that it is tracking,
        // in other words, if Entity framework is tracking any changes to our entities in memory
        public bool HasChanges()
        {
            return _context.ChangeTracker.HasChanges();
        }
    }
}