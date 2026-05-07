using FluentValidation;
using Microsoft.Extensions.Localization;

namespace Services.Features.Auth.Commands.ConfirmEmail
{
    public class ConfirmEmailCommandValidator : AbstractValidator<ConfirmEmailCommand>
    {
        public ConfirmEmailCommandValidator(IStringLocalizer<Domain.Resources.Messages> localizer)
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage(localizer["Auth_EmailRequired"])
                .EmailAddress().WithMessage(localizer["Auth_EmailInvalid"]);

            RuleFor(x => x.Token)
                .NotEmpty().WithMessage(localizer["Auth_TokenRequired"]);
        }
    }
}
