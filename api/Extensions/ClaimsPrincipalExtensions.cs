using System.Security.Claims;

namespace api.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        // we extend ClaimsPrincipal class, so we can get username from a token easier
        public static string GetUsername(this ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}