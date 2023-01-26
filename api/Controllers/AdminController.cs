using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    public class AdminController : BaseApiController
    {
        // in this controller we configure some policies for endpoints,
        // policies are more flexible than using [Authorize(Roles = "Admin")],
        // policies configurations are in IdentityServiceExtensions class

        [Authorize(Policy = "RequireAdminRole")]
        [HttpGet("users-with-roles")] // /api/admin/users-with-roles
        public ActionResult GetUsersWithRoles()
        {
            return Ok("Only admins can see this");
        }

        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpGet("photos-to-moderate")] // /api/admin/photos-to-moderate
        public ActionResult GetPhotosForModeration()
        {
            return Ok("Admins or moderators can see this");
        }
    }
}