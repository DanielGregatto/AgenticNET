using Domain.Contracts.Common;
using Domain.Enums;
using Domain.Interfaces;
using FluentValidation;
using Identity.Model;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Services.Core;
using Services.Interfaces;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Services.Features.Auth.Commands.ExternalLoginCallback
{
    public class ExternalLoginCallbackCommandHandler : BaseCommandHandler,
        IRequestHandler<ExternalLoginCallbackCommand, Result<string>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly IValidator<ExternalLoginCallbackCommand> _validator;
        private readonly IStringLocalizer<Domain.Resources.Messages> _localizer;

        public ExternalLoginCallbackCommandHandler(
            Data.Context.AppDbContext context,
            IUser user,

            UserManager<ApplicationUser> userManager,
            IJwtTokenGenerator jwtTokenGenerator,
            IValidator<ExternalLoginCallbackCommand> validator,
            IStringLocalizer<Domain.Resources.Messages> localizer)
            : base(context, user)
        {
            _userManager = userManager;
            _jwtTokenGenerator = jwtTokenGenerator;
            _validator = validator;
            _localizer = localizer;
        }

        public async Task<Result<string>> Handle(ExternalLoginCallbackCommand request, CancellationToken cancellationToken)
        {
            // Explicit validation using helper
            var validationError = await ValidateAsync<ExternalLoginCallbackCommand, string>(_validator, request, cancellationToken);
            if (validationError != null)
                return validationError;

            if (!request.AuthenticateResult.Succeeded || request.AuthenticateResult.Principal == null)
            {
                return Result<string>.Failure(_localizer["Auth_ExternalAuthFailed"], ErrorTypes.Unauthorized);
            }

            var email = request.AuthenticateResult.Principal.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email))
            {
                return Result<string>.Failure(_localizer["Auth_ExternalProviderEmailNotFound"], ErrorTypes.Validation);
            }

            var user = await _userManager.FindByEmailAsync(email);

            // If user exists but email is not confirmed, delete and recreate
            if (user != null && !user.EmailConfirmed)
            {
                var deleteResult = await _userManager.DeleteAsync(user);
                if (!deleteResult.Succeeded)
                {
                    return Result<string>.Failure(_localizer["Auth_OldUserCleanupFailed"], ErrorTypes.Database);
                }
                user = null;
            }

            // Create new user if doesn't exist
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };

                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    var errors = createResult.Errors.Select(e => new Error(e.Description, ErrorTypes.Validation)).ToArray();
                    return Result<string>.ValidationFailure(errors);
                }
            }

            // Add external login if not already added
            var logins = await _userManager.GetLoginsAsync(user);
            if (logins.All(l => l.LoginProvider != request.Provider))
            {
                var loginInfo = new UserLoginInfo(request.Provider, user.Id, request.Provider.ToUpperInvariant());
                await _userManager.AddLoginAsync(user, loginInfo);
            }

            var token = await _jwtTokenGenerator.GenerateAccessTokenAsync(user);
            return Result<string>.Success(token);
        }
    }
}
