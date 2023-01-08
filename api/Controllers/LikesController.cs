using api.DTOs;
using api.Entities;
using api.Extensions;
using api.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    public class LikesController : BaseApiController
    {
        private readonly IUserRepository _userRepository;
        private readonly ILikesRepository _likesRepository;
        public LikesController(IUserRepository userRepository, ILikesRepository likesRepository)
        {
            _likesRepository = likesRepository;
            _userRepository = userRepository;
            
        }

        [HttpPost("{username}")] // api/likes/{username}
        public async Task<ActionResult> AddLike(string username)
        {
            // this is the user that's going to be liking another user
            var sourceUserId = int.Parse(User.GetUserId());

            var likedUser = await _userRepository.GetUserByUsernameAsync(username);
            var sourceUser = await _likesRepository.GetUserWithLikes(sourceUserId);

            if (likedUser == null) return NotFound();

            if (sourceUser.UserName == username) return BadRequest("You cannot like yourself");

            var userLike = await _likesRepository.GetUserLike(sourceUserId, likedUser.Id);

            // userLike must be null, which means than nothing found at database
            if (userLike != null) return BadRequest("You already like this user");

            userLike = new UserLike 
            {
                SourceUserId = sourceUserId,
                TargetUserId = likedUser.Id
            };

            // create this entry in the Likes table 
            sourceUser.LikedUsers.Add(userLike);

            // it seems wrong, but it works
            if (await _userRepository.SaveAllAsync()) return Ok();

            return BadRequest("Failed to like user");
        }

        [HttpGet] // api/likes?predicate=liked or api/likes?predicate=likedBy
        public async Task<ActionResult<IEnumerable<LikeDto>>> GetUserLikes(string predicate)
        {
            // User.GetUserId() method comes from claims principal extensions
            var users = await _likesRepository.GetUserLikes(predicate, int.Parse(User.GetUserId()));

            return Ok(users);
        }
    }
}