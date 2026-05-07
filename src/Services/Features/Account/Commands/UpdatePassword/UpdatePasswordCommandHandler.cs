using Domain.Contracts.Common;
using Domain.Enums;
using Domain.Interfaces;
using FluentValidation;
using Identity.Model;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Services.Core;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Services.Features.Account.Commands.UpdatePassword
{
    public class UpdatePasswordCommandHandler : BaseCommandHandler,
        IRequestHandler<UpdatePasswordCommand, Result<string>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<UpdatePasswordCommandHandler> _logger;
        private readonly IValidator<UpdatePasswordCommand> _validator;
        private readonly IStringLocalizer<Domain.Resources.Messages> _localizer;

        public UpdatePasswordCommandHandler(
            Data.Context.AppDbContext context,
            IUser user,

            UserManager<ApplicationUser> userManager,
            ILogger<UpdatePasswordCommandHandler> logger,
            IValidator<UpdatePasswordCommand> validator,
            IStringLocalizer<Domain.Resources.Messages> localizer)
            : base(context, user)
        {
            _userManager = userManager;
            _logger = logger;
            _validator = validator;
            _localizer = localizer;
        }

        public async Task<Result<string>> Handle(UpdatePasswordCommand request, CancellationToken cancellationToken)
        {
            var validationError = await ValidateAsync<UpdatePasswordCommand, string>(_validator, request, cancellationToken);
            if (validationError != null)
                return validationError;

            var user = await _userManager.FindByIdAsync(UserID.ToString());

            if (user == null)
                return Result<string>.NotFound(_localizer["Account_UserNotFound"]);

            // Check if current password is valid
            var isCurrentPasswordValid = await _userManager.CheckPasswordAsync(user, request.CurrentPassword);
            if (!isCurrentPasswordValid)
                return Result<string>.ValidationFailure(new Error(_localizer["Account_CurrentPasswordIncorrect"], ErrorTypes.Validation));

            // Check if new password is different from current password
            if (request.NewPassword.Equals(request.CurrentPassword))
                return Result<string>.ValidationFailure(new Error(_localizer["Account_NewPasswordMustBeDifferent"], ErrorTypes.Validation));

            // Change password
            var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

            if (!result.Succeeded)
            {
                _logger.LogError("Failed to update password for user {UserId}", UserID);
                var errors = result.Errors.Select(e => new Error(e.Description, ErrorTypes.Validation)).ToArray();
                return Result<string>.ValidationFailure(errors);
            }

            return Result<string>.Success(_localizer["Account_PasswordChanged"]);
        }
    }
}
