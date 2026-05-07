using Domain.Contracts.Common;
using Services.Contracts.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using System;

namespace Services.Features.Product.Commands.UpdateProduct
{
    public class UpdateProductCommand : IRequest<Result<ProductResult>>
    {
        public Guid Id { get; set; }
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
