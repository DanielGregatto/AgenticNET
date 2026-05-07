using System.Security.Claims;

namespace Identity.Model
{
    public static class ClaimsPrincipalExtensions
    {
        public static string? GetUserId(this ClaimsPrincipal principal)
        {
            if (principal == null)
            {
                throw new ArgumentException(nameof(principal));
            }

            var claim = principal.FindFirst(ClaimTypes.NameIdentifier);
            return claim?.Value;
        }

        public static string? GetClaim(this IEnumerable<Claim> Claims, string Type)
        {
            return Claims.FirstOrDefault(x => x.Type == Type)?.Value;
        }
    }
}
