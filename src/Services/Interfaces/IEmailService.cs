using Identity.Model;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface IEmailService : IEmailSender<ApplicationUser>
    {
        /// <summary>
        /// Adds the specified email address to the collection.
        /// </summary>
        /// <param name="email">The email address to add. Cannot be null or empty.</param>
        void AddTo(string email);

        /// <summary>
        /// Removes all items from the collection and adds them to a separate list for further processing or inspection.
        /// </summary>
        /// <remarks>After calling this method, the original collection is empty. The removed items are
        /// preserved in a list, which can be accessed through a related property or method, depending on the
        /// implementation.</remarks>
        void CleanToList();

        /// <summary>
        /// Sends an email message with the specified subject and HTML-formatted body.
        /// </summary>
        /// <param name="subject">The subject line of the email message. Cannot be null or empty.</param>
        /// <param name="html">The HTML content to include in the body of the email message. Cannot be null or empty.</param>
        void SendHtml(string subject, string html);

        /// <summary>
        /// Sends an email message with the specified subject and HTML-formatted body content asynchronously.
        /// </summary>
        /// <param name="subject">The subject line of the email message. Cannot be null or empty.</param>
        /// <param name="html">The HTML content to include in the body of the email message. Cannot be null or empty.</param>
        /// <returns>A task that represents the asynchronous send operation.</returns>
        Task SendHtmlAsync(string subject, string html);

        /// <summary>
        /// Sends the specified email and password to register a new user with a third-party authentication provider.
        /// </summary>
        /// <param name="email">The email address to associate with the new user account. Cannot be null or empty.</param>
        /// <param name="password">The password to use for the new user account. Cannot be null or empty.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task SendCredentialsThirdPartyRegister(string email, string password);
    }
}