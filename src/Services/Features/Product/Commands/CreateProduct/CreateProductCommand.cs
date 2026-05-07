using Domain.Contracts.Common;
using Services.Contracts.Results;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Services.Features.Product.Commands.CreateProduct
{
    public class CreateProductCommand : IRequest<Result<ProductResult>>
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Slug { get; set; }
        public decimal Price { get; set; }
        public string Sku { get; set; }
        public int Stock { get; set; }
        public IFormFile Image { get; set; }
        public bool Active { get; set; }
    }
}
