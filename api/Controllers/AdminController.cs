using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Query;

namespace api.Controllers
{
    public class AdminController : BaseApiController
    {
        public ActionResult GetUsersWithRoles()
        {
            return Ok("Only admins can see this");
        }

        public ActionResult GetPhotosForModeration()
        {
            return Ok("Admins or moderators can see this");
        }
    }
}