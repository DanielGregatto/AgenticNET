using Identity.Model;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface IJwtTokenGenerator
    {
        /// <summary>
        /// Generates a new access token for the specified user asynchronously.
        /// </summary>
        /// <param name="user">The user for whom to generate the access token. Cannot be <c>null</c>.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the generated access token as a
        /// string.</returns>
        Task<string> GenerateAccessTokenAsync(ApplicationUser user);

        /// <summary>
        /// Generates a new refresh token for the specified user asynchronously.
        /// </summary>
        /// <param name="user">The user for whom to generate the refresh token. Cannot be <c>null</c>.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the newly generated refresh
        /// token as a string.</returns>
        Task<string> GenerateRefreshTokenAsync(ApplicationUser user);
    }
}
