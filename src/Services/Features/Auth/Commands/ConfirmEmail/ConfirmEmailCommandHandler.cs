using Domain.Contracts.Common;
using Domain.Enums;
using Domain.Interfaces;
using FluentValidation;
using Identity.Model;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Services.Core;
using Services.Interfaces;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Services.Features.Auth.Commands.ConfirmEmail
{
    public class ConfirmEmailCommandHandler : BaseCommandHandler,
        IRequestHandler<ConfirmEmailCommand, Result<UriBuilder>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly JWTConfig _jwtConfig;
        private readonly IValidator<ConfirmEmailCommand> _validator;
        private readonly IStringLocalizer<Domain.Resources.Messages> _localizer;

        public ConfirmEmailCommandHandler(
            Data.Context.AppDbContext context,
            IUser user,

            UserManager<ApplicationUser> userManager,
            IJwtTokenGenerator jwtTokenGenerator,
            IOptions<JWTConfig> jwtConfig,
            IValidator<ConfirmEmailCommand> validator,
            IStringLocalizer<Domain.Resources.Messages> localizer)
            : base(context, user)
        {
            _userManager = userManager;
            _jwtTokenGenerator = jwtTokenGenerator;
            _jwtConfig = jwtConfig.Value;
            _validator = validator;
            _localizer = localizer;
        }

        public async Task<Result<UriBuilder>> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
        {
            // Explicit validation using helper
            var validationError = await ValidateAsync<ConfirmEmailCommand, UriBuilder>(_validator, request, cancellationToken);
            if (validationError != null)
                return validationError;

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return Result<UriBuilder>.NotFound(_localizer["Auth_UserNotFound"]);
            }

            var result = await _userManager.ConfirmEmailAsync(user, request.Token);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => new Error(e.Description, ErrorTypes.Validation)).ToList();
                errors.Add(new Error(_localizer["Auth_InvalidEmailConfirmation"], ErrorTypes.Validation));
                return Result<UriBuilder>.ValidationFailure(errors.ToArray());
            }

            var jwtToken = await _jwtTokenGenerator.GenerateAccessTokenAsync(user);
            var redirectUri = new UriBuilder(_jwtConfig.RedirectUriEmailConfirm)
            {
                Query = $"token={Uri.EscapeDataString(jwtToken)}"
            };

            return Result<UriBuilder>.Success(redirectUri);
        }
    }
}
