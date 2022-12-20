using System.Security.Claims;
using api.Data;
using api.DTOs;
using api.Entities;
using api.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api.Controllers
{
    [Authorize]
    public class UsersController : BaseApiController
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public UsersController(IUserRepository userRepository, IMapper mapper)
        {
            _mapper = mapper;
            _userRepository = userRepository;
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
            var username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;          
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
    }
}