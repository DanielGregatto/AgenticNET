using FluentValidation;
using Microsoft.Extensions.Localization;
using System;
using Util.Interfaces;

namespace Services.Features.Account.Commands.UpdatePersonalInfo
{
    public class UpdatePersonalInfoCommandValidator : AbstractValidator<UpdatePersonalInfoCommand>
    {
        public UpdatePersonalInfoCommandValidator(
            IStringLocalizer<Domain.Resources.Messages> localizer,
            IHandlerValidation handlerValidation)
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage(localizer["Account_FullNameRequired"])
                .MaximumLength(100).WithMessage(localizer["Account_FullNameMaxLength"])
                .Must(name => !string.IsNullOrWhiteSpace(name) && name.Contains(" "))
                    .WithMessage(localizer["Account_FullNameMustIncludeFirstAndLastName"]);

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage(localizer["Account_PhoneRequired"])
                .Must(handlerValidation.IsValidPhoneNumber).WithMessage(localizer["Account_PhoneInvalid"]);

            RuleFor(x => x.CPF_CNPJ)
                .NotEmpty().WithMessage(localizer["Account_CpfRequired"])
                .Must(handlerValidation.IsValidCPFOrCNPJ).WithMessage(localizer["Account_CpfCnpjInvalid"]);

            RuleFor(x => x.DateOfBirth)
                .LessThan(DateTime.Now).WithMessage(localizer["Account_DateOfBirthInvalid"])
                .When(x => x.DateOfBirth.HasValue);
        }
    }
}