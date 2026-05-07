using Domain.Interfaces;
using Identity.Model;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace CrossCutting.Identity.Models
{
    public class AspNetUser : IUser
    {
        private readonly IHttpContextAccessor _accessor;

        public AspNetUser(IHttpContextAccessor accessor)
        {
            _accessor = accessor;
        }

        public string Name => _accessor.HttpContext?.User?.Identity?.Name ?? string.Empty;

        public bool IsAuthenticated()
        {
            return _accessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
        }

        public Guid GetUserId()
        {
            if (!IsAuthenticated())
                return Guid.Empty;

            var user = _accessor.HttpContext?.User;
            if (user == null)
                return Guid.Empty;

            var userId = user.GetUserId();
            return Guid.TryParse(userId, out var guid) ? guid : Guid.Empty;
        }

        public IEnumerable<Claim> GetClaimsIdentity()
        {
            return _accessor.HttpContext?.User?.Claims ?? Enumerable.Empty<Claim>();
        }
    }
}