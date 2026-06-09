using Domain.Contracts.API;
using Services.Contracts.Results;
using Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Features.Product.Commands.CreateProduct;
using Services.Features.Product.Commands.DeleteProduct;
using Services.Features.Product.Commands.UpdateProduct;
using Services.Features.Product.Queries.GetAllProducts;
using Services.Features.Product.Queries.GetProductById;
using Services.Features.Product.Queries.SearchProductsPaginated;
using UI.API.Controllers.Base;

namespace UI.API.Controllers
{
    /// <summary>
    /// Product catalog CRUD. Read operations are open (no token required); write operations
    /// (create, update, delete) require a Bearer JWT.
    /// </summary>
    public class ProductController : CoreController
    {
        private readonly IMediatorHandler _mediator;
        private readonly IUser _user;
        private readonly ILogger<ProductController> _logger;

        public ProductController(IMediatorHandler mediator, IUser user, ILogger<ProductController> logger)
        {
            _mediator = mediator;
            _user = user;
            _logger = logger;
        }

        /// <summary>
        /// Return all products in the catalog. No authentication required.
        /// </summary>
        [HttpGet("v1/products")]
        [ProducesResponseType(typeof(SuccessResponse<IEnumerable<ProductResult>>), 200)]
        [ProducesResponseType(typeof(ErrorResponseDto), 401)]
        [ProducesResponseType(typeof(ErrorResponseDto), 403)]
        [ProducesResponseType(typeof(ErrorResponseDto), 404)]
        public async Task<IActionResult> GetAll()
        {
            _logger.LogInformation("Getting all products");

            var query = new GetAllProductsQuery();
            var result = await _mediator.SendCommand(query);

            if (result.IsSuccess)
                _logger.LogInformation("Successfully retrieved {Count} products", result.Data?.Count() ?? 0);
            else
                _logger.LogWarning("Failed to retrieve products. Errors: {Errors}",
                    string.Join(", ", result.Errors.Select(e => e.Message)));

            return Response(result);
        }

        /// <summary>
        /// Return a single product by its ID. No authentication required.
        /// </summary>
        /// <param name="id">The product's unique identifier (GUID).</param>
        [HttpGet("v1/products/{id}")]
        [ProducesResponseType(typeof(SuccessResponse<ProductResult>), 200)]
        [ProducesResponseType(typeof(ErrorResponseDto), 401)]
        [ProducesResponseType(typeof(ErrorResponseDto), 403)]
        [ProducesResponseType(typeof(ErrorResponseDto), 404)]
        public async Task<IActionResult> GetById(Guid id)
        {
            _logger.LogInformation("Getting product by ID: {ProductId}", id);

            var query = new GetProductByIdQuery { Id = id };
            var result = await _mediator.SendCommand(query);

            if (result.IsSuccess)
                _logger.LogInformation("Successfully retrieved product {ProductId}", id);
            else
                _logger.LogWarning("Failed to retrieve product {ProductId}. Errors: {Errors}",
                    id, string.Join(", ", result.Errors.Select(e => e.Message)));

            return Response(result);
        }

        /// <summary>
        /// Search products with pagination and optional sorting. No authentication required.
        /// </summary>
        /// <param name="pageIndex">1-based page number. Defaults to 1.</param>
        /// <param name="pageSize">Items per page. Defaults to 10.</param>
        /// <param name="orderByProperty">Property name to sort by. Omit for default order.</param>
        /// <param name="orderByDescending">Set to true for descending order. Defaults to false.</param>
        [HttpGet("v1/products/search")]
        [ProducesResponseType(typeof(SuccessResponse<PaginatedResponseDto<ProductResult>>), 200)]
        [ProducesResponseType(typeof(ErrorResponseDto), 401)]
        [ProducesResponseType(typeof(ErrorResponseDto), 403)]
        [ProducesResponseType(typeof(ErrorResponseDto), 404)]
        public async Task<IActionResult> SearchPaginated(
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string orderByProperty = null,
            [FromQuery] bool orderByDescending = false)
        {
            _logger.LogInformation("Searching products with pageIndex: {PageIndex}, pageSize: {PageSize}", pageIndex, pageSize);

            var query = new SearchProductsPaginatedQuery
            {
                PageIndex = pageIndex,
                PageSize = pageSize,
                OrderByProperty = orderByProperty,
                OrderByDescending = orderByDescending
            };
            var result = await _mediator.SendCommand(query);

            if (result.IsSuccess)
                _logger.LogInformation("Successfully retrieved {Count} products (page {PageIndex} of {TotalPages})",
                    result.Data?.Items?.Count() ?? 0, pageIndex, (result.Data?.TotalCount ?? 0) / pageSize + 1);
            else
                _logger.LogWarning("Failed to search products. Errors: {Errors}",
                    string.Join(", ", result.Errors.Select(e => e.Message)));

            return Response(result);
        }

        /// <summary>
        /// Create a new product. Accepts multipart/form-data (supports image upload). Requires a Bearer JWT.
        /// </summary>
        /// <param name="command">Product fields including optional image file.</param>
        [HttpPost("v1/products")]
        [Authorize]
        [ProducesResponseType(typeof(SuccessResponse<ProductResult>), 200)]
        [ProducesResponseType(typeof(ErrorResponseDto), 401)]
        [ProducesResponseType(typeof(ErrorResponseDto), 403)]
        public async Task<IActionResult> Create([FromForm] CreateProductCommand command)
        {
            _logger.LogInformation("Creating new product with name: {ProductName}", command.Name);

            var result = await _mediator.SendCommand(command);

            if (result.IsSuccess)
                _logger.LogInformation("Successfully created product with name: {ProductName}", command.Name);
            else
                _logger.LogWarning("Failed to create product. Errors: {Errors}",
                    string.Join(", ", result.Errors.Select(e => e.Message)));

            return Response(result);
        }

        /// <summary>
        /// Update an existing product. Accepts multipart/form-data (supports image upload). Requires a Bearer JWT.
        /// </summary>
        /// <param name="id">ID of the product to update.</param>
        /// <param name="command">Updated product fields.</param>
        [HttpPut("v1/products/{id}")]
        [Authorize]
        [ProducesResponseType(typeof(SuccessResponse<ProductResult>), 200)]
        [ProducesResponseType(typeof(ErrorResponseDto), 401)]
        [ProducesResponseType(typeof(ErrorResponseDto), 403)]
        [ProducesResponseType(typeof(ErrorResponseDto), 404)]
        public async Task<IActionResult> Update(Guid id, [FromForm] UpdateProductCommand command)
        {
            _logger.LogInformation("Updating product {ProductId} with name: {ProductName}", id, command.Name);

            command.Id = id;
            var result = await _mediator.SendCommand(command);

            if (result.IsSuccess)
                _logger.LogInformation("Successfully updated product {ProductId}", id);
            else
                _logger.LogWarning("Failed to update product {ProductId}. Errors: {Errors}",
                    id, string.Join(", ", result.Errors.Select(e => e.Message)));

            return Response(result);
        }

        /// <summary>
        /// Delete a product by ID. Requires a Bearer JWT.
        /// </summary>
        /// <param name="id">ID of the product to delete.</param>
        [HttpDelete("v1/products/{id}")]
        [Authorize]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(ErrorResponseDto), 401)]
        [ProducesResponseType(typeof(ErrorResponseDto), 403)]
        [ProducesResponseType(typeof(ErrorResponseDto), 404)]
        public async Task<IActionResult> Delete(Guid id)
        {
            _logger.LogInformation("Deleting product {ProductId}", id);

            var command = new DeleteProductCommand { Id = id };
            var result = await _mediator.SendCommand(command);

            if (result.IsSuccess)
                _logger.LogInformation("Successfully deleted product {ProductId}", id);
            else
                _logger.LogWarning("Failed to delete product {ProductId}. Errors: {Errors}",
                    id, string.Join(", ", result.Errors.Select(e => e.Message)));

            return Response(result);
        }
    }
}
