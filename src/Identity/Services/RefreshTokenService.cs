using Identity.Context;
using Identity.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Identity.Services
{
    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly AppIdentityDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public RefreshTokenService(AppIdentityDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<UserRefreshToken> CreateAsync(ApplicationUser user)
        {
            var token = Guid.NewGuid().ToString("N");
            var refreshToken = new UserRefreshToken
            {
                UserId = user.Id,
                Token = token,
                ExpiryDate = DateTime.UtcNow.AddDays(7)
            };

            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();
            return refreshToken;
        }

        public async Task<bool> RevokeAsync(string token)
        {
            var existing = await _context.RefreshTokens.FirstOrDefaultAsync(x => x.Token == token);
            if (existing == null) return false;

            existing.IsRevoked = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsValidAsync(string token, string userId)
        {
            return await _context.RefreshTokens.AnyAsync(x =>
                x.Token == token &&
                x.UserId == userId &&
                !x.IsRevoked &&
                x.ExpiryDate > DateTime.UtcNow);
        }

        public async Task<UserRefreshToken> GetByTokenAsync(string token)
        {
            return await _context.RefreshTokens
                                .Include(r => r.User)
                                .FirstOrDefaultAsync(x => x.Token == token && !x.IsRevoked);
        }

        public async Task<UserRefreshToken> GetAsync(string UserId, string token)
        {
            return await _context.RefreshTokens
                                .Include(r => r.User)
                                .FirstOrDefaultAsync(x => x.UserId == UserId && x.Token == token && !x.IsRevoked);
        }
    }
}
