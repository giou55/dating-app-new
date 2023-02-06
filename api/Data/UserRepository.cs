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

        public async Task<MemberDto> GetMemberAsync(string username, bool isCurrentUser)
        {
            var query = _context.Users
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
                .AsQueryable();

                // need to ignore Query filter for the current user
                if (isCurrentUser) query = query.IgnoreQueryFilters();

                return await query.FirstOrDefaultAsync();
        }

        public async Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams)
        {
            var query = _context.Users.AsQueryable();

            // now we're going to built up our query based on the userParams
            query = query.Where(u => u.UserName != userParams.CurrentUsername); // exclude the current user
            query = query.Where(u => u.Gender == userParams.Gender); // get only the specific gender

            // minDob refers to the oldest member that we want to get,
            // so we do a subtraction (this year - max age) to find the dob of the oldest member
            var minDob = DateOnly.FromDateTime(DateTime.Today.AddYears(-userParams.MaxAge - 1));

            // maxDob refers to the younger member that we want to get,
            // so we do a subtraction (this year - min age) to find the dob of the younger member
            var maxDob = DateOnly.FromDateTime(DateTime.Today.AddYears(-userParams.MinAge));

            query = query.Where(u => u.DateOfBirth >= minDob && u.DateOfBirth <= maxDob);

            query = userParams.OrderBy switch
            {
                "created" => query.OrderByDescending(u => u.Created),
                // this is the default
                _ => query.OrderByDescending(u => u.LastActive)
            };

            // we use this static async method from PagedList.cs to execute queries against the database
            return await PagedList<MemberDto>.CreateAsync(
                // here not need to write: 
                // Include(p => p.Photos)
                query.ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
                    // it means that Entity framework isn't going to keep track 
                    // of what we return from this method
                    .AsNoTracking(), 
                userParams.PageNumber, 
                userParams.PageSize
            );     
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

        public async Task<string> GetUserGender(string username)
        {
            return await _context.Users
                .Where(x => x.UserName == username)
                .Select(x => x.Gender)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<AppUser>> GetUsersAsync()
        {
            return await _context.Users
                .Include(p => p.Photos)
                .ToListAsync();
        }

        // we're using UnitOfWork, so we don't need to implement this from interface,
        // now we're going to rely on the Complete method from the UnitOfWork 
        // public async Task<bool> SaveAllAsync()
        // {
        //     return await _context.SaveChangesAsync() > 0;
        // }

        public void Update(AppUser user)
        {
            _context.Entry(user).State = EntityState.Modified;
        }

        public async Task<AppUser> GetUserByPhotoId(int photoId)
        {
            return await _context.Users
                .Include(p => p.Photos)
                .IgnoreQueryFilters()
                .Where(p => p.Photos.Any(p => p.Id == photoId))
                .FirstOrDefaultAsync();
        }
    }
}