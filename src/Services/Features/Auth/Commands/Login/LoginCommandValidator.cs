using FluentValidation;
using Microsoft.Extensions.Localization;

namespace Services.Features.Auth.Commands.Login
{
    public class LoginCommandValidator : AbstractValidator<LoginCommand>
    {
        public LoginCommandValidator(IStringLocalizer<Domain.Resources.Messages> localizer)
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage(localizer["Auth_EmailRequired"])
                .EmailAddress().WithMessage(localizer["Auth_EmailInvalid"]);

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage(localizer["Auth_PasswordRequired"]);
        }
    }
}
