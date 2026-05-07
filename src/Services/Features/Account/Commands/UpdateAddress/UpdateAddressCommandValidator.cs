using FluentValidation;
using Microsoft.Extensions.Localization;

namespace Services.Features.Account.Commands.UpdateAddress
{
    public class UpdateAddressCommandValidator : AbstractValidator<UpdateAddressCommand>
    {
        public UpdateAddressCommandValidator(IStringLocalizer<Domain.Resources.Messages> localizer)
        {
            RuleFor(x => x.Cep)
                .NotEmpty().WithMessage(localizer["Account_CepRequired"])
                .Matches(@"^\d{8}$").WithMessage(localizer["Account_CepFormat"]);

            RuleFor(x => x.Street)
                .NotEmpty().WithMessage(localizer["Account_StreetRequired"])
                .MaximumLength(255).WithMessage(localizer["Account_StreetMaxLength"]);

            RuleFor(x => x.Number)
                .NotEmpty().WithMessage(localizer["Account_NumberRequired"]);

            RuleFor(x => x.Complement)
                .MaximumLength(255).WithMessage(localizer["Account_ComplementMaxLength"])
                .When(x => !string.IsNullOrWhiteSpace(x.Complement));

            RuleFor(x => x.Neighborhood)
                .NotEmpty().WithMessage(localizer["Account_NeighborhoodRequired"])
                .MaximumLength(100).WithMessage(localizer["Account_NeighborhoodMaxLength"]);

            RuleFor(x => x.City)
                .NotEmpty().WithMessage(localizer["Account_CityRequired"])
                .MaximumLength(100).WithMessage(localizer["Account_CityMaxLength"]);

            RuleFor(x => x.State)
                .NotEmpty().WithMessage(localizer["Account_StateRequired"])
                .Length(2).WithMessage(localizer["Account_StateLength"]);
        }
    }
}