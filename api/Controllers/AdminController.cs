using System.Reflection.Metadata.Ecma335;
using api.Entities;
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

        public AdminController(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
            
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
        public ActionResult GetPhotosForModeration()
        {
            return Ok("Admins or moderators can see this");
        }
    }
}