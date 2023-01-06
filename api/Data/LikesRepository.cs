using api.DTOs;
using api.Entities;
using api.Interfaces;

namespace api.Data
{
    public class LikesRepository : ILikesRepository
    {
        public Task<UserLike> GetUserLike(int sourceUserId, int targetUserId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<LikeDto>> GetUserLikes(string predicate, int userId)
        {
            throw new NotImplementedException();
        }

        public Task<AppUser> GetUserWithLikes(int userId)
        {
            throw new NotImplementedException();
        }
    }
}