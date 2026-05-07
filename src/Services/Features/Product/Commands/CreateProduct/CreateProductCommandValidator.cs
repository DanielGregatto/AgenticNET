using FluentValidation;
using Microsoft.Extensions.Localization;

namespace Services.Features.Product.Commands.CreateProduct
{
    public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
    {
        public CreateProductCommandValidator(IStringLocalizer<Domain.Resources.Messages> localizer)
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage(localizer["RequiredField", nameof(CreateProductCommand.Name)])
                .MaximumLength(1000).WithMessage(localizer["MaxLength", nameof(CreateProductCommand.Name), 1000]);

            RuleFor(x => x.Slug)
                .NotEmpty().WithMessage(localizer["RequiredField", nameof(CreateProductCommand.Slug)])
                .MaximumLength(1000).WithMessage(localizer["MaxLength", nameof(CreateProductCommand.Slug), 1000]);

            RuleFor(x => x.Price)
                .GreaterThanOrEqualTo(0).WithMessage(localizer["InvalidValue", nameof(CreateProductCommand.Price)]);

            RuleFor(x => x.Stock)
                .GreaterThanOrEqualTo(0).WithMessage(localizer["InvalidValue", nameof(CreateProductCommand.Stock)]);

            RuleFor(x => x.Sku)
                .MaximumLength(1000).WithMessage(localizer["MaxLength", nameof(CreateProductCommand.Sku), 1000]);
        }
    }
}
