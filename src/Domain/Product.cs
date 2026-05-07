using Domain.Core;
using Domain.Resources;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Domain
{
    public class Product : EntityBase<Product>, IValidatableObject
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Slug { get; set; }
        public decimal Price { get; set; }
        public string Sku { get; set; }
        public int Stock { get; set; }
        public string ImageFileName { get; set; }
        public bool Active { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var localizer = validationContext.GetService<IStringLocalizer<Messages>>();

            if (string.IsNullOrWhiteSpace(Name))
                yield return new ValidationResult(localizer?["RequiredField", nameof(Name)], new[] { nameof(Name) });

            if (string.IsNullOrWhiteSpace(Slug))
                yield return new ValidationResult(localizer?["RequiredField", nameof(Slug)], new[] { nameof(Slug) });

            if (Price < 0)
                yield return new ValidationResult(localizer?["InvalidValue", nameof(Price)], new[] { nameof(Price) });

            if (Stock < 0)
                yield return new ValidationResult(localizer?["InvalidValue", nameof(Stock)], new[] { nameof(Stock) });
        }
    }
}
