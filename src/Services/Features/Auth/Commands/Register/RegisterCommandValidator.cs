using FluentValidation;
using Microsoft.Extensions.Localization;

namespace Services.Features.Auth.Commands.Register
{
    public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
    {
        public RegisterCommandValidator(IStringLocalizer<Domain.Resources.Messages> localizer)
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage(localizer["Auth_EmailRequired"])
                .EmailAddress().WithMessage(localizer["Auth_EmailInvalid"]);

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage(localizer["Auth_PasswordRequired"])
                .MinimumLength(6).WithMessage(localizer["Auth_PasswordMinLength"]);

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty().WithMessage(localizer["Auth_ConfirmPasswordRequired"])
                .Equal(x => x.Password).WithMessage(localizer["Auth_PasswordsMismatch"]);
        }
    }
}
