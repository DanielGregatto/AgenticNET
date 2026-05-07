using Data.Context;
using Domain.Contracts.Common;
using Domain.Interfaces;
using System;

namespace Services.Core
{
    public abstract class BaseQueryHandler
    {
        protected Guid UserID { get; private set; }
        protected readonly AppDbContext _context;
        protected readonly IUser _user;

        protected BaseQueryHandler(AppDbContext context, IUser user)
        {
            _context = context;
            _user = user;

            if (user.IsAuthenticated())
                UserID = user.GetUserId();
        }

        /// <summary>
        /// Checks whether the current user is authenticated.
        /// </summary>
        /// <remarks>This method determines authentication based on whether the <c>UserID</c> is set to a
        /// non-empty value.</remarks>
        /// <returns>A <see cref="Result"/> indicating the authentication status of the current user. Returns <see
        /// cref="Result.Success"/> if the user is authenticated; otherwise, returns <see cref="Result.Unauthorized"/>.</returns>
        protected Result CheckUserAuthentication()
        {
            if (UserID == Guid.Empty)
                return Result.Unauthorized("Para realizar está ação você precisa estar logado");

            return Result.Success();
        }
    }
}
