using FluentValidation;
using Microsoft.Extensions.Localization;

namespace Services.Features.Account.Commands.UpdatePassword
{
    public class UpdatePasswordCommandValidator : AbstractValidator<UpdatePasswordCommand>
    {
        public UpdatePasswordCommandValidator(IStringLocalizer<Domain.Resources.Messages> localizer)
        {
            RuleFor(x => x.CurrentPassword)
                .NotEmpty().WithMessage(localizer["Account_CurrentPasswordRequired"]);

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage(localizer["Account_NewPasswordRequired"])
                .MinimumLength(6).WithMessage(localizer["Account_NewPasswordMinLength"])
                .MaximumLength(100).WithMessage(localizer["Account_NewPasswordMaxLength"]);

            RuleFor(x => x.ConfirmNewPassword)
                .NotEmpty().WithMessage(localizer["Account_ConfirmPasswordRequired"])
                .Equal(x => x.NewPassword).WithMessage(localizer["Account_PasswordsDoNotMatch"]);
        }
    }
}
