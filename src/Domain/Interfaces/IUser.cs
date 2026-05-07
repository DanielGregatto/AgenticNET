using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace Domain.Interfaces
{
    public interface IUser
    {
        /// <summary>
        /// Gets the name of the currently authenticated user.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Determines whether the current HTTP request is associated with an authenticated user.
        /// </summary>
        /// <returns><see langword="true"/> if the user making the current HTTP request is authenticated; otherwise, <see
        /// langword="false"/>.</returns>
        public bool IsAuthenticated();

        /// <summary>
        /// Retrieves the unique identifier of the currently authenticated user.
        /// </summary>
        /// <remarks>Returns <see cref="Guid.Empty"/> if the user is not authenticated or if a valid user
        /// identifier cannot be determined from the current HTTP context.</remarks>
        /// <returns>A <see cref="Guid"/> representing the user's unique identifier if the user is authenticated and the
        /// identifier is available; otherwise, <see cref="Guid.Empty"/>.</returns>
        public Guid GetUserId();

        /// <summary>
        /// Retrieves the collection of claims associated with the current HTTP user.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{Claim}"/> containing the claims for the current user.  Returns an empty collection
        /// if there is no active HTTP context or user.</returns>
        public IEnumerable<Claim> GetClaimsIdentity();
    }
}