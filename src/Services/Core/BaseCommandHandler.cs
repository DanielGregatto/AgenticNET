using Data.Context;
using Domain.Contracts.Common;
using Domain.Interfaces;
using FluentValidation;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Services.Core
{
    public abstract class BaseCommandHandler
    {
        protected Guid UserID { get; private set; }
        protected readonly AppDbContext _context;
        protected readonly IUser _user;

        protected BaseCommandHandler(
            AppDbContext context,
            IUser user)
        {
            _context = context;
            _user = user;

            if (user.IsAuthenticated())
                UserID = user.GetUserId();
        }

        /// <summary>
        /// Checks whether the current user is authenticated.
        /// </summary>
        /// <returns>A <see cref="Result"/> indicating the authentication status of the user. Returns an unauthorized result if
        /// the user is not logged in; otherwise, returns a success result.</returns>
        protected Result CheckUserAuthentication()
        {
            if (UserID == Guid.Empty)
                return Result.Unauthorized("Para realizar está ação você precisa estar logado");

            return Result.Success();
        }

        /// <summary>
        /// Asynchronously validates a request using the specified validator.
        /// </summary>
        /// <remarks>This method performs asynchronous validation of the specified request. If validation
        /// fails, it returns a <see cref="Result{TResponse}"/> containing the validation errors. If validation
        /// succeeds, it returns <see langword="null"/> to indicate that no validation errors were found.</remarks>
        /// <typeparam name="TRequest">The type of the request object to validate.</typeparam>
        /// <typeparam name="TResponse">The type of the response associated with the validation result.</typeparam>
        /// <param name="validator">The validator to use for validating the <paramref name="request"/>. Cannot be <c>null</c>.</param>
        /// <param name="request">The request object to validate. The requirements for this object are defined by the <paramref
        /// name="validator"/>.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the validation operation.</param>
        /// <returns>A <see cref="Result{TResponse}"/> representing a validation failure if the request is invalid; otherwise,
        /// <see langword="null"/> if the request is valid.</returns>
        protected async Task<Result<TResponse>?> ValidateAsync<TRequest, TResponse>(
            IValidator<TRequest> validator,
            TRequest request,
            CancellationToken cancellationToken)
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);

            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .Select(e => Error.Validation(e.ErrorMessage, e.PropertyName))
                    .ToArray();
                return Result<TResponse>.ValidationFailure(errors);
            }

            return null;
        }
    }
}
