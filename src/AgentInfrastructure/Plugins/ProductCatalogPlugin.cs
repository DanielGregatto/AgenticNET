using Domain.Interfaces;
using Microsoft.SemanticKernel;
using Services.Features.Product.Queries.GetAllProducts;
using Services.Features.Product.Queries.GetProductById;
using Services.Features.Product.Queries.SearchProductsPaginated;
using System;
using System.ComponentModel;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace AgentInfrastructure.Plugins
{
    public class ProductCatalogPlugin : IProductCatalogPlugin
    {
        private readonly IMediatorHandler _mediator;

        public ProductCatalogPlugin(IMediatorHandler mediator)
        {
            _mediator = mediator;
        }

        [KernelFunction, Description("List all active products in the catalog")]
        public async Task<string> ListProducts(CancellationToken cancellationToken = default)
        {
            var result = await _mediator.SendCommand(new GetAllProductsQuery());

            if (result.IsFailure)
                return "Unable to retrieve the product list.";

            return JsonSerializer.Serialize(result.Data);
        }

        [KernelFunction, Description("Get detailed information about a specific product by its ID")]
        public async Task<string> GetProductById(
            [Description("The product ID in GUID format, e.g. 3fa85f64-5717-4562-b3fc-2c963f66afa6")] string id,
            CancellationToken cancellationToken = default)
        {
            if (!Guid.TryParse(id, out var guid))
                return "Invalid product ID format. Please provide a valid GUID.";

            var result = await _mediator.SendCommand(new GetProductByIdQuery(guid));

            if (result.IsFailure)
                return $"Product with ID '{id}' was not found.";

            return JsonSerializer.Serialize(result.Data);
        }

        [KernelFunction, Description("Search for products by keyword matched against product name or description")]
        public async Task<string> SearchProducts(
            [Description("Keyword to search for in product name or description")] string keyword,
            CancellationToken cancellationToken = default)
        {
            var result = await _mediator.SendCommand(new SearchProductsPaginatedQuery
            {
                PageIndex = 1,
                PageSize = 20,
                Predicate = p => p.Name.Contains(keyword) ||
                                 (p.Description != null && p.Description.Contains(keyword))
            });

            if (result.IsFailure)
                return "Unable to search products.";

            return JsonSerializer.Serialize(result.Data?.Items);
        }
    }
}
