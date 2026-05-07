using FluentValidation;
using Microsoft.Extensions.Localization;

namespace Services.Features.Auth.Commands.ExternalLoginCallback
{
    public class ExternalLoginCallbackCommandValidator : AbstractValidator<ExternalLoginCallbackCommand>
    {
        public ExternalLoginCallbackCommandValidator(IStringLocalizer<Domain.Resources.Messages> localizer)
        {
            RuleFor(x => x.Provider)
                .NotEmpty().WithMessage(localizer["Auth_ProviderRequired"]);

            RuleFor(x => x.AuthenticateResult)
                .NotNull().WithMessage(localizer["Auth_AuthResultRequired"]);
        }
    }
}
