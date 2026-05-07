using FluentValidation;
using Microsoft.Extensions.Localization;

namespace Services.Features.Auth.Commands.ForgotPassword
{
    public class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
    {
        public ForgotPasswordCommandValidator(IStringLocalizer<Domain.Resources.Messages> localizer)
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage(localizer["Auth_EmailRequired"])
                .EmailAddress().WithMessage(localizer["Auth_EmailInvalid"]);
        }
    }
}
