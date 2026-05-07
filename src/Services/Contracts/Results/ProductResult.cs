using Domain;
using System;

namespace Services.Contracts.Results
{
    public record ProductResult(
        Guid Id,
        string Name,
        string Description,
        string Slug,
        decimal Price,
        string Sku,
        int Stock,
        string ImageFileName,
        bool Active,
        DateTime CreatedAt)
    {
        public static ProductResult FromEntity(Product product)
        {
            if (product == null) return null;

            return new ProductResult(
                product.Id,
                product.Name ?? "",
                product.Description ?? "",
                product.Slug ?? "",
                product.Price,
                product.Sku ?? "",
                product.Stock,
                product.ImageFileName ?? "",
                product.Active,
                product.CreatedAt
            );
        }
    }
}
