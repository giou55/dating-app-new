using Microsoft.AspNetCore.Mvc;

// our API need to fallback to, if it doesn't know how to handle a specific route

namespace api.Controllers
{
    // we don't use BaseApiController this time,
    // we use Controller because we need access to a view,
    // and we're going to tell it to go to a physical file,
    // which is the index.html inside api/wwwroot folder,
    // if it doesn't know how to handle a specific route
    public class FallbackController : Controller
    {
        public ActionResult Index()
        {
            return PhysicalFile(Path.Combine(
                Directory.GetCurrentDirectory(), "wwwroot", "index.html"), "text/HTML");
        }
    }
}