using api.DTOs;
using api.Entities;

namespace api.Interfaces
{
    public interface ILikesRepository
    {
        // we pass two properties that make up the primary key of the entity 
        // that lives inside the Likes table
        Task<UserLike> GetUserLike(int sourceUserId, int targetUserId);

        Task<AppUser> GetUserWithLikes(int userId);

        // predicate indicates if we want to get the users that likes
        // or the users that are liked by  
        Task<IEnumerable<LikeDto>> GetUserLikes(string predicate, int userId);
    }
}