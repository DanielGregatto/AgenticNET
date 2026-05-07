using FluentValidation;
using Microsoft.Extensions.Localization;

namespace Services.Features.Product.Commands.UpdateProduct
{
    public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
    {
        public UpdateProductCommandValidator(IStringLocalizer<Domain.Resources.Messages> localizer)
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage(localizer["RequiredField", nameof(UpdateProductCommand.Id)]);

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage(localizer["RequiredField", nameof(UpdateProductCommand.Name)])
                .MaximumLength(1000).WithMessage(localizer["MaxLength", nameof(UpdateProductCommand.Name), 1000]);

            RuleFor(x => x.Slug)
                .NotEmpty().WithMessage(localizer["RequiredField", nameof(UpdateProductCommand.Slug)])
                .MaximumLength(1000).WithMessage(localizer["MaxLength", nameof(UpdateProductCommand.Slug), 1000]);

            RuleFor(x => x.Price)
                .GreaterThanOrEqualTo(0).WithMessage(localizer["InvalidValue", nameof(UpdateProductCommand.Price)]);

            RuleFor(x => x.Stock)
                .GreaterThanOrEqualTo(0).WithMessage(localizer["InvalidValue", nameof(UpdateProductCommand.Stock)]);

            RuleFor(x => x.Sku)
                .MaximumLength(1000).WithMessage(localizer["MaxLength", nameof(UpdateProductCommand.Sku), 1000]);
        }
    }
}
