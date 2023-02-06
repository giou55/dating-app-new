using api.Entities;
using api.Interfaces;
using api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api.Controllers
{
    public class AdminController : BaseApiController
    {
        // in this controller we configure some policies for endpoints,
        // policies are more flexible than using [Authorize(Roles = "Admin")],
        // policies configurations are in IdentityServiceExtensions class
        private readonly UserManager<AppUser> _userManager;
        private readonly IUnitOfWork _uow;
        private readonly IPhotoService _photoService;


        public AdminController(
            UserManager<AppUser> userManager, 
            IUnitOfWork uow,
            IPhotoService photoService)
        {
            _userManager = userManager;
            _uow = uow;
            _photoService = photoService;
        }

        [Authorize(Policy = "RequireAdminRole")]
        [HttpGet("users-with-roles")] // /api/admin/users-with-roles
        public async Task<ActionResult> GetUsersWithRoles()
        {
            // we get users as a list of objects,
            // and each object has this format:
            // {
            //    "id": 7,
            //    "username": "baker",
            //    "roles": [
            //       "Member"
            //    ]
            // }
            var users = await _userManager.Users
                .OrderBy(u => u.UserName)
                // we return an anonymous object
                .Select(u => new
                {
                    u.Id,
                    Username = u.UserName,
                    Roles = u.UserRoles.Select(r => r.Role.Name).ToList()
                })
                .ToListAsync();

            return Ok(users);
        }

        [Authorize(Policy = "RequireAdminRole")]
        [HttpPost("edit-roles/{username}")] // /api/admin/edit-roles/{username}?roles=Moderator,Member
        public async Task<ActionResult> EditRoles(string username, [FromQuery]string roles)
        {
            if (string.IsNullOrEmpty(roles)) return BadRequest("You must select at least one role");

            // roles is string separated by commas
            var selectedRoles = roles.Split(",").ToArray();

            var user = await _userManager.FindByNameAsync(username);

            if (user == null) return NotFound();

            // we get the existing roles of the user
            var userRoles = await _userManager.GetRolesAsync(user); // ["Member","Moderator"]

            // we add the selected roles to the user, except the roles that the user is already in 
            var result = await _userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));

            if (!result.Succeeded) return BadRequest("Failed to add to roles");

            // we remove the existing roles to the user, except the selected roles 
            result = await _userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));

            if (!result.Succeeded) return BadRequest("Failed to remove from roles");
            // we send back the current roles of the user
            return Ok(await _userManager.GetRolesAsync(user)); // ["Member","Moderator"]
        }

        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpGet("photos-to-moderate")] // /api/admin/photos-to-moderate
        public async Task<ActionResult> GetPhotosForModeration()
        {
            var photos = await _uow.PhotoRepository.GetUnapprovedPhotos();
            return Ok(photos);
        }

        // [Authorize(Policy = "ModeratePhotoRole")]
        // [HttpPost("approve-photo/{photoId}")]
        // public async Task<ActionResult> ApprovePhoto(int photoId)
        // {
        //     var photo = await _uow.PhotoRepository.GetPhotoById(photoId);
        //     photo.IsApproved = true;
        //     await _uow.Complete();
        //     return Ok();
        // }

        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpPost("approve-photo/{photoId}")]
        public async Task<ActionResult> ApprovePhoto(int photoId)
        {
            var photo = await _uow.PhotoRepository.GetPhotoById(photoId);

            if (photo == null) return NotFound("Could not find photo");

            photo.IsApproved = true;

            var user = await _uow.UserRepository.GetUserByPhotoId(photoId);

            if (!user.Photos.Any(x => x.IsMain)) photo.IsMain = true;

            await _uow.Complete();
            return Ok();
        }

        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpPost("reject-photo/{photoId}")]
        public async Task<ActionResult> RejectPhoto(int photoId)
        {
            var photo = await _uow.PhotoRepository.GetPhotoById(photoId);

            if (photo.PublicId != null)
            {
                var result = await _photoService.DeletePhotoAsync(photo.PublicId);
                if (result.Result == "ok")
                {
                    _uow.PhotoRepository.RemovePhoto(photo);
                }
            }
            else
            {
                _uow.PhotoRepository.RemovePhoto(photo);
            }

            await _uow.Complete();
            return Ok();
        }
    }
}