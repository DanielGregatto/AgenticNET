using Microsoft.AspNetCore.Http;

namespace Util.Interfaces
{
    public interface IHandlerBrowser
    {
        /// <summary>
        /// Determines whether the specified HTTP request originates from a mobile browser.
        /// </summary>
        /// <param name="request">The HTTP request to evaluate for a mobile user agent.</param>
        /// <returns><see langword="true"/> if the request appears to be from a mobile browser; otherwise, <see
        /// langword="false"/>.</returns>
        bool IsMobileBrowser(HttpRequest request);

        /// <summary>
        /// Retrieves the value of the "User-Agent" header from the specified HTTP request.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequest"/> from which to obtain the "User-Agent" header. Cannot be <see
        /// langword="null"/>.</param>
        /// <returns>The value of the "User-Agent" header if present; otherwise, <see langword="null"/>.</returns>
        string UserAgent(HttpRequest request);
    }
}