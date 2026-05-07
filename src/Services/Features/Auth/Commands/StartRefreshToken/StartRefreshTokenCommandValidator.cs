using FluentValidation;
using Microsoft.Extensions.Localization;
using System;

namespace Services.Features.Auth.Commands.StartRefreshToken
{
    public class StartRefreshTokenCommandValidator : AbstractValidator<StartRefreshTokenCommand>
    {
        public StartRefreshTokenCommandValidator(IStringLocalizer<Domain.Resources.Messages> localizer)
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage(localizer["Auth_UserIdRequired"])
                .NotEqual(Guid.Empty).WithMessage(localizer["Auth_UserIdInvalid"]);
        }
    }
}
