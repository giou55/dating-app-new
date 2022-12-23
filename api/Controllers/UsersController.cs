using System.Security.Claims;
using api.DTOs;
using api.Entities;
using api.Extensions;
using api.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Authorize]
    public class UsersController : BaseApiController
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IPhotoService _photoService;

        public UsersController
        (
            IUserRepository userRepository,
            IMapper mapper,
            IPhotoService photoService
        )
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _photoService = photoService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers()
        {
            var users = await _userRepository.GetMembersAsync();
            // remove this because mapping now happened in UserRepository.cs
            // var usersToReturn = _mapper.Map<IEnumerable<MemberDto>>(users);
            return Ok(users);
        }

        [HttpGet("{username}")]
        public async Task<ActionResult<MemberDto>> GetUser(string username)
        {
            return await _userRepository.GetMemberAsync(username);
            // remove this because mapping now happened in UserRepository.cs
            // return _mapper.Map<MemberDto>(user);
        }

        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
        {
            // we created a new extension method to get username from a token
            //var username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // we use a extension method to get username
            var username = User.GetUsername();

            var user = await _userRepository.GetUserByUsernameAsync(username);

            if (user == null) return NotFound();

            // the properties of memberUpdateDto are overwriting the properties of user
            _mapper.Map(memberUpdateDto, user);

            // NoContent means status code 204, everything OK and nothing to send back
            if (await _userRepository.SaveAllAsync()) return NoContent();

            // failed to update user because there were no changes to be saved,
            // so we return a 400 bad request with a message
            return BadRequest("Failed to update user");
        }

        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
        {
            // we use a extension method to get username
            var username = User.GetUsername();
            var user = await _userRepository.GetUserByUsernameAsync(username);

            if (user == null) return NotFound();

            var result = await _photoService.AddPhotoAsync(file); 

            if (result.Error != null) return BadRequest(result.Error.Message);

            var photo = new Photo
            {
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId
            };

            // if it is the first photo that user uploads, set photo the main photo
            if (user.Photos.Count == 0) photo.IsMain = true;

            user.Photos.Add(photo);

            if (await _userRepository.SaveAllAsync()) 
            {
                // wrong response when we create a new resource
                //return _mapper.Map<PhotoDto>(photo);

                // it returns a 201 created response along with location header about 
                // where to find the newly created resource
                return CreatedAtAction(
                    nameof(GetUser), 
                    new {username = user.UserName}, 
                    // also sends back the newly created resource
                    _mapper.Map<PhotoDto>(photo)
                );
            }

            return BadRequest("Problem adding photo");
        }
    }
}