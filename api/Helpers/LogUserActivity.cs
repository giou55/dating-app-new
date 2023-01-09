using api.Extensions;
using api.Interfaces;
using Microsoft.AspNetCore.Mvc.Filters;

namespace api.Helpers
{
    // this is a action filter class for updating the LastActive property of user after each request, 
    // and we register it as a service inside ApplicationServiceExtensions.cs file,
    // and we specify it inside BaseApiController.cs file
    public class LogUserActivity : IAsyncActionFilter
    {
        // we can do something before next or after next
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // here we'll do something after the API has done its job
            var resultContext = await next();

            // we want to do something only when the user is logged in
            if (!resultContext.HttpContext.User.Identity.IsAuthenticated) return;

            var userId = resultContext.HttpContext.User.GetUserId();

            // also we need access to the repository, because we're going to update something for our user
            var repo = resultContext.HttpContext.RequestServices.GetRequiredService<IUserRepository>();

            // now we can update the LastActive property of user
            var user = await repo.GetUserByIdAsync(userId);
            user.LastActive = DateTime.UtcNow;
            await repo.SaveAllAsync();
        }
    }
}