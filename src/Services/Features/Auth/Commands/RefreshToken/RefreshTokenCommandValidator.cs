using FluentValidation;
using Microsoft.Extensions.Localization;

namespace Services.Features.Auth.Commands.RefreshToken
{
    public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
    {
        public RefreshTokenCommandValidator(IStringLocalizer<Domain.Resources.Messages> localizer)
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage(localizer["Auth_UserIdRequired"]);

            RuleFor(x => x.RefreshToken)
                .NotEmpty().WithMessage(localizer["Auth_RefreshTokenRequired"]);
        }
    }
}
