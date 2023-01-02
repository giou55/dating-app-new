using api.DTOs;
using api.Entities;
using api.Helpers;
using api.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace api.Data
{
    public class UserRepository : IUserRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        public UserRepository(DataContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context; 
        }

        public async Task<MemberDto> GetMemberAsync(string username)
        {
            return await _context.Users
                .Where(x => x.UserName == username)
                // using Automapper service for mapping
                .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
                // too complicated to use this approach without mapping
                // .Select(user => new MemberDto
                // {
                //     Id = user.Id,
                //     UserName = user.UserName,
                //     KnownAs = user.KnownAs,
                //     Gender = user.Gender
                //     Introduction = user.Introduction
                //     LookingFor = user.LookingFor
                //     Interests = user.Interests
                // })
                .SingleOrDefaultAsync();
        }

        public async Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams)
        {
            var query = _context.Users
                // here not need to write: 
                // Include(p => p.Photos)
                .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
                // it means that Entity framework isn't going to keep track 
                // of what we return from this method
                .AsNoTracking();

            // we use this static async method from PagedList.cs to execute queries against the database
            return await PagedList<MemberDto>.CreateAsync(query, userParams.PageNumber, userParams.PageSize);     
        }

        public async Task<AppUser> GetUserByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<AppUser> GetUserByUsernameAsync(string username)
        {
            return await _context.Users
                .Include(p => p.Photos)
                .SingleOrDefaultAsync(x => x.UserName == username);
        }

        public async Task<IEnumerable<AppUser>> GetUsersAsync()
        {
            return await _context.Users
                .Include(p => p.Photos)
                .ToListAsync();
        }

        public async Task<bool> SaveAllAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public void Update(AppUser user)
        {
            _context.Entry(user).State = EntityState.Modified;
        }
    }
}