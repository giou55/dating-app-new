using api.DTOs;
using api.Entities;
using api.Extensions;
using api.Interfaces;
using Microsoft.EntityFrameworkCore;
using SQLitePCL;

namespace api.Data
{
    public class LikesRepository : ILikesRepository
    {
        private readonly DataContext _context;
        public LikesRepository(DataContext context)
        {
            _context = context;
            
        }

        public async Task<UserLike> GetUserLike(int sourceUserId, int targetUserId)
        {
            // the primary key for Likes table is made up of two integers
            return await _context.Likes.FindAsync(sourceUserId, targetUserId);
        }

        // the userId parameter can be the source userId or the target userId
        public async Task<IEnumerable<LikeDto>> GetUserLikes(string predicate, int userId)
        {
            var users = _context.Users.OrderBy(u => u.UserName).AsQueryable(); // nothing is being executed at database yet 
            var likes = _context.Likes.AsQueryable(); // nothing is being executed at database yet

            // we want the users that the current user likes
            if (predicate == "liked") 
            {
                likes = likes.Where(like => like.SourceUserId == userId); // nothing is being executed at database yet
                users = likes.Select(like => like.TargetUser); // nothing is being executed at database yet
            }

            // we want the users that like the current user
            if (predicate == "likedBy") 
            {
                likes = likes.Where(like => like.TargetUserId == userId); // nothing is being executed at database yet
                users = likes.Select(like => like.SourceUser); // nothing is being executed at database yet
            }

            // Queryable.Select method projects each element of a sequence into a new form
            // If users is not Queryable, we will write:
            // users.AsQueryable().Select()
            return await users.Select(user => new LikeDto
            {
                UserName = user.UserName,
                KnownAs = user.KnownAs,
                Age = user.DateOfBirth.CalculateAge(),
                PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain).Url,
                City = user.City,
                Id = user.Id
            }).ToListAsync();
        }

        // this is going to allow us to check to see if a user already has been liked by another user
        public async Task<AppUser> GetUserWithLikes(int userId)
        {
            return await _context.Users 
                .Include(x => x.LikedUsers)
                .FirstOrDefaultAsync(x => x.Id == userId);
        }
    }
}