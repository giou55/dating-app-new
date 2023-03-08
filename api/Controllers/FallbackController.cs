using Microsoft.AspNetCore.Mvc;

// our API need to fallback to, if it doesn't know how to handle a specific route

namespace api.Controllers
{
    public class FallbackController : Controller
    {
        public ActionResult Index()
        {
            return PhysicalFile(Path.Combine(
                Directory.GetCurrentDirectory(), "wwwroot", "index.html"), "text/HTML");
        }
    }
}