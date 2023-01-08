using System.Security.Claims;

namespace api.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        // we extend ClaimsPrincipal class, so we can call HttpContext.User.GetUsername() 
        // to get the username from the token easier
        public static string GetUsername(this ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.Name)?.Value;
        }

        // we extend ClaimsPrincipal class, so we can call HttpContext.User.GetUserId()  
        // to get the userId from the token easier
        public static string GetUserId(this ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}