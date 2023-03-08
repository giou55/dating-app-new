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
        private readonly IUnitOfWork _uow;
        public LikesController(IUnitOfWork uow)
        {
            _uow = uow;
        }

        [HttpPost("{username}")] // api/likes/{username}
        public async Task<ActionResult> AddLike(string username)
        {
            var sourceUserId = User.GetUserId();

            var likedUser = await _uow.UserRepository.GetUserByUsernameAsync(username);
            var sourceUser = await _uow.LikesRepository.GetUserWithLikes(sourceUserId);

            if (likedUser == null) return NotFound();

            if (sourceUser.UserName == username) return BadRequest("Δεν μπορείτε να κάνετε like στον εαυτό σας");

            var userLike = await _uow.LikesRepository.GetUserLike(sourceUserId, likedUser.Id);

            if (userLike != null) return BadRequest("Έχετε ήδη κάνει like στον χρήστη");

            userLike = new UserLike
            {
                SourceUserId = sourceUserId,
                TargetUserId = likedUser.Id
            };

            sourceUser.LikedUsers.Add(userLike);

            if (await _uow.Complete()) return Ok();

            return BadRequest("Failed to like user");
        }

        [HttpGet] // api/likes?predicate=liked or api/likes?predicate=likedBy
        public async Task<ActionResult<PagedList<LikeDto>>> GetUserLikes([FromQuery]LikesParams likesParams)
        {
            likesParams.UserId = User.GetUserId();

            var users = await _uow.LikesRepository.GetUserLikes(likesParams);

            Response.AddPaginationHeader(new PaginationHeader(
                users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages));

            return Ok(users);
        }
    }
}