using Domain.Contracts.Common;
using Domain.Enums;
using Domain.Interfaces;
using FluentValidation;
using Identity.Model;
using Identity.Model.Responses;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Services.Core;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Services.Features.Auth.Commands.ResetPassword
{
    public class ResetPasswordCommandHandler : BaseCommandHandler,
        IRequestHandler<ResetPasswordCommand, Result<ResetPasswordDto>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IValidator<ResetPasswordCommand> _validator;
        private readonly IStringLocalizer<Domain.Resources.Messages> _localizer;

        public ResetPasswordCommandHandler(
            Data.Context.AppDbContext context,
            IUser user,

            UserManager<ApplicationUser> userManager,
            IValidator<ResetPasswordCommand> validator,
            IStringLocalizer<Domain.Resources.Messages> localizer)
            : base(context, user)
        {
            _userManager = userManager;
            _validator = validator;
            _localizer = localizer;
        }

        public async Task<Result<ResetPasswordDto>> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            // Explicit validation using helper
            var validationError = await ValidateAsync<ResetPasswordCommand, ResetPasswordDto>(_validator, request, cancellationToken);
            if (validationError != null)
                return validationError;

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return Result<ResetPasswordDto>.NotFound(_localizer["Auth_UserNotFound"]);
            }

            var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => new Error(e.Description, ErrorTypes.Validation)).ToArray();
                return Result<ResetPasswordDto>.ValidationFailure(errors);
            }

            return Result<ResetPasswordDto>.Success(new ResetPasswordDto(_localizer["Auth_PasswordResetSuccessful"]));
        }
    }
}
