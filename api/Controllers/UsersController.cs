using System.Security.Claims;
using api.DTOs;
using api.Entities;
using api.Extensions;
using api.Helpers;
using api.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Authorize]
    public class UsersController : BaseApiController
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly IPhotoService _photoService;

        public UsersController
        (
            IUnitOfWork uow,
            IMapper mapper,
            IPhotoService photoService
        )
        {
            _uow = uow;
            _mapper = mapper;
            _photoService = photoService;
        }

        [HttpGet]
        public async Task<ActionResult<PagedList<MemberDto>>> GetUsers([FromQuery]UserParams userParams)
        {
            var gender = await _uow.UserRepository.GetUserGender(User.GetUsername());
            userParams.CurrentUsername = User.GetUsername();

            if (string.IsNullOrEmpty(userParams.Gender)) {
                userParams.Gender = gender == "male" ? "female" : "male"; // we want the opposite gender
            }

            var users = await _uow.UserRepository.GetMembersAsync(userParams);

            Response.AddPaginationHeader(new PaginationHeader(
                    users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages)
            );

            return Ok(users);
        }

        [HttpGet("{username}")]
        public async Task<ActionResult<MemberDto>> GetUser(string username)
        {
            var currentUsername = User.GetUsername();

            return await _uow.UserRepository
                .GetMemberAsync(username, isCurrentUser: currentUsername == username);
        }

        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
        {
            var username = User.GetUsername();

            var user = await _uow.UserRepository.GetUserByUsernameAsync(username);

            if (user == null) return NotFound();

            _mapper.Map(memberUpdateDto, user);

            if (await _uow.Complete()) return NoContent();

            return BadRequest("Failed to update user");
        }

        [HttpDelete("delete-user/{username}")]
        public async Task<ActionResult> DeleteUser(string username)
        {
            var user = await _uow.UserRepository.GetUserByUsernameAsync(username);
            if (user == null) return NotFound();
            _uow.UserRepository.Remove(user);
             if (await _uow.Complete()) return Ok();
            return BadRequest("Problem deleting the user");
        }

        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
        {
            var username = User.GetUsername();
            var user = await _uow.UserRepository.GetUserByUsernameAsync(username);

            if (user == null) return NotFound();

            var result = await _photoService.AddPhotoAsync(file);

            if (result.Error != null) return BadRequest(result.Error.Message);

            var photo = new Photo
            {
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId
            };

            user.Photos.Add(photo);

            if (await _uow.Complete())
            {

                return CreatedAtAction(
                    nameof(GetUser),
                    new {username = user.UserName},
                    _mapper.Map<PhotoDto>(photo)
                );
            }

            return BadRequest("Problem adding photo");
        }

        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> SetMainPhoto(int photoId)
        {
            var username = User.GetUsername();
            var user = await _uow.UserRepository.GetUserByUsernameAsync(username);

            if (user == null) return NotFound();

            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

            if (photo == null) return NotFound();

            if (photo.IsMain) return BadRequest("This is already your main photo");

            var currentMain = user.Photos.FirstOrDefault(x => x.IsMain);
            if (currentMain != null) currentMain.IsMain = false;
            photo.IsMain = true;

            if (await _uow.Complete()) return NoContent();

            return BadRequest("Problem setting the main photo");
        }

        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            var username = User.GetUsername();
            var user = await _uow.UserRepository.GetUserByUsernameAsync(username);

            var photo = await _uow.PhotoRepository.GetPhotoById(photoId);

            if (photo == null) return NotFound();

            if (photo.IsMain) return BadRequest("You cannot delete your main photo");

            if (photo.PublicId != null)
            {
                var result = await _photoService.DeletePhotoAsync(photo.PublicId);
                if (result.Error != null) return BadRequest(result.Error.Message);
            }

            user.Photos.Remove(photo);

            if (await _uow.Complete()) return Ok();

            return BadRequest("Problem deleting photo");
        }
    }
}