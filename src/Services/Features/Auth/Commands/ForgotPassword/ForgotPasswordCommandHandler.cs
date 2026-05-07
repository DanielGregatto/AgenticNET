using Domain.Contracts.Common;
using Domain.Enums;
using Domain.Interfaces;
using FluentValidation;
using Identity.Model;
using Identity.Model.Responses;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Services.Core;
using Services.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Services.Features.Auth.Commands.ForgotPassword
{
    public class ForgotPasswordCommandHandler : BaseCommandHandler,
        IRequestHandler<ForgotPasswordCommand, Result<ForgotPasswordDto>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly JWTConfig _jwtConfig;
        private readonly IValidator<ForgotPasswordCommand> _validator;
        private readonly IStringLocalizer<Domain.Resources.Messages> _localizer;

        public ForgotPasswordCommandHandler(
            Data.Context.AppDbContext context,
            IUser user,

            UserManager<ApplicationUser> userManager,
            IEmailService emailService,
            IOptions<JWTConfig> jwtConfig,
            IValidator<ForgotPasswordCommand> validator,
            IStringLocalizer<Domain.Resources.Messages> localizer)
            : base(context, user)
        {
            _userManager = userManager;
            _emailService = emailService;
            _jwtConfig = jwtConfig.Value;
            _validator = validator;
            _localizer = localizer;
        }

        public async Task<Result<ForgotPasswordDto>> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
        {
            // Explicit validation using helper
            var validationError = await ValidateAsync<ForgotPasswordCommand, ForgotPasswordDto>(_validator, request, cancellationToken);
            if (validationError != null)
                return validationError;

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return Result<ForgotPasswordDto>.NotFound(_localizer["Auth_UserNotFound"]);
            }

            if (!user.EmailConfirmed)
            {
                return Result<ForgotPasswordDto>.ValidationFailure(
                    new Error(_localizer["Auth_EmailNotConfirmedBeforePasswordRecovery"], ErrorTypes.Validation));
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetLink = $"{_jwtConfig.RedirectUriResetPassword}?email={Uri.EscapeDataString(user.Email)}&token={Uri.EscapeDataString(token)}";
            await _emailService.SendPasswordResetLinkAsync(user, user.Email, resetLink);

            return Result<ForgotPasswordDto>.Success(
                new ForgotPasswordDto(_localizer["Auth_PasswordRecoveryLinkSent"], token));
        }
    }
}
