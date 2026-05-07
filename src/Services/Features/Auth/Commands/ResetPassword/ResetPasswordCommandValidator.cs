using FluentValidation;
using Microsoft.Extensions.Localization;

namespace Services.Features.Auth.Commands.ResetPassword
{
    public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
    {
        public ResetPasswordCommandValidator(IStringLocalizer<Domain.Resources.Messages> localizer)
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage(localizer["Auth_EmailRequired"])
                .EmailAddress().WithMessage(localizer["Auth_EmailInvalid"]);

            RuleFor(x => x.Token)
                .NotEmpty().WithMessage(localizer["Auth_TokenRequired"]);

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage(localizer["Auth_NewPasswordRequired"])
                .MinimumLength(6).WithMessage(localizer["Auth_NewPasswordMinLength"]);

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty().WithMessage(localizer["Auth_ConfirmPasswordRequired"])
                .Equal(x => x.NewPassword).WithMessage(localizer["Auth_PasswordsMismatch"]);
        }
    }
}
