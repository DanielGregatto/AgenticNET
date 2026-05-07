using Identity.Model;

namespace Identity.Services
{
    public interface IRefreshTokenService
    {
        Task<UserRefreshToken> CreateAsync(ApplicationUser user);
        Task<bool> RevokeAsync(string token);
        Task<bool> IsValidAsync(string token, string userId);
        Task<UserRefreshToken> GetByTokenAsync(string token);
        Task<UserRefreshToken> GetAsync(string UserId, string token);
    }
}
