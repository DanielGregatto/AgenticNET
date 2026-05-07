using System.Threading.Tasks;

namespace Services.Interfaces
{
    /// <summary>
    /// Defines a service for asynchronously validating turnstile tokens.
    /// </summary>
    /// <remarks>Implementations of this interface provide a mechanism to verify the validity of tokens, such
    /// as those used for authentication or access control. The validation process is performed asynchronously to
    /// support non-blocking operations, such as remote or network-based checks.</remarks>
    public interface ITurnstileValidatorService
    {
        /// <summary>
        /// Asynchronously validates the specified token and determines whether it is valid.
        /// </summary>
        /// <param name="token">The token to validate. Cannot be <see langword="null"/> or empty.</param>
        /// <returns>A task that represents the asynchronous validation operation. The task result is <see langword="true"/> if
        /// the token is valid; otherwise, <see langword="false"/>.</returns>
        Task<bool> ValidateAsync(string token);
    }
}