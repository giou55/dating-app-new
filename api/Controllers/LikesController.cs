using api.DTOs;
using api.Entities;
using api.Extensions;
using api.Helpers;
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
            var sourceUserId = User.GetUserId();

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
        // we add [FromQuery] because likesParams is an object instead of a string
        // and our API controller can't bind easily to it, 
        // so we need to tell it where to find these parameters 
        public async Task<ActionResult<PagedList<LikeDto>>> GetUserLikes([FromQuery]LikesParams likesParams)
        {
            // inside likesParams we'll not have the UserId, so we get it from claims principal extensions 
            likesParams.UserId = User.GetUserId();

            var users = await _likesRepository.GetUserLikes(likesParams);

            Response.AddPaginationHeader(new PaginationHeader(
                users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages));

            return Ok(users);
        }
    }
}