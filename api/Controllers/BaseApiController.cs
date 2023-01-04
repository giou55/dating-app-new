using api.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    // we specify the action filter that we want to use   
    [ServiceFilter(typeof(LogUserActivity))]
    [ApiController]
    [Route("api/[controller]")]
    public class BaseApiController : ControllerBase
    {
        
    }
}