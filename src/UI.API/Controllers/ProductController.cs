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
        /// Retrieves all products available in the system.
        /// </summary>
        /// <remarks>This endpoint returns a collection of products. Access may be restricted based on
        /// user permissions.</remarks>
        /// <returns>An <see cref="IActionResult"/> containing a <see cref="SuccessResponse{T}"/> with a list of <see
        /// cref="ProductResult"/> objects if successful; otherwise, an <see cref="ErrorResponseDto"/> describing the
        /// error.</returns>
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
        /// Retrieves the details of a product by its unique identifier.
        /// </summary>
        /// <remarks>This endpoint returns a <see cref="SuccessResponse{T}"/> containing the product
        /// information if the specified product exists and the caller is authorized. If the product is not found, or if
        /// the caller lacks the necessary permissions, an appropriate error response is returned.</remarks>
        /// <param name="id">The unique identifier of the product to retrieve.</param>
        /// <returns>An <see cref="IActionResult"/> containing a <see cref="SuccessResponse{ProductDto}"/> with the product
        /// details if found; otherwise, an <see cref="ErrorResponseDto"/> describing the error.</returns>
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
        /// Searches for products and returns a paginated list of results.
        /// </summary>
        /// <remarks>This endpoint supports pagination and optional sorting of product results. The caller
        /// can specify the page index, page size, and sorting options to control the returned data. Only accessible to
        /// authorized users.</remarks>
        /// <param name="pageIndex">The one-based index of the page to retrieve. Must be greater than or equal to 1. Defaults to 1.</param>
        /// <param name="pageSize">The number of products to include in each page. Must be greater than 0. Defaults to 10.</param>
        /// <param name="orderByProperty">The name of the product property to sort by. If <see langword="null"/>, the default sort order is applied.</param>
        /// <param name="orderByDescending"><see langword="true"/> to sort the results in descending order; otherwise, <see langword="false"/> for
        /// ascending order. Defaults to <see langword="false"/>.</param>
        /// <returns>An <see cref="IActionResult"/> containing a <see cref="SuccessResponse{T}"/> with a <see
        /// cref="PaginatedResponseDto{ProductDto}"/> of products if successful; otherwise, an <see
        /// cref="ErrorResponseDto"/> describing the error.</returns>
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
        /// Creates a new product using the specified command data.
        /// </summary>
        /// <remarks>This endpoint requires authentication and authorization. The product information must
        /// be provided in the form data. On success, returns the details of the created product; otherwise, returns an
        /// error response indicating the reason for failure.</remarks>
        /// <param name="command">The command containing the product data to create. Must include all required product fields; cannot be null.</param>
        /// <returns>An <see cref="IActionResult"/> containing a <see cref="SuccessResponse{ProductDto}"/> with the created
        /// product details if successful, or an <see cref="ErrorResponseDto"/> describing the error if the operation
        /// fails or the user is unauthorized.</returns>
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
        /// Updates the details of an existing product identified by the specified ID.
        /// </summary>
        /// <remarks>Requires authentication and appropriate authorization. Returns a success response
        /// with the updated product information if the operation completes successfully. If the product is not found,
        /// or if the user lacks permission, an error response is returned.</remarks>
        /// <param name="id">The unique identifier of the product to update. Must correspond to an existing product.</param>
        /// <param name="command">The command containing the updated product data. Must include all required fields for the update operation.</param>
        /// <returns>An <see cref="IActionResult"/> containing a <see cref="SuccessResponse{ProductDto}"/> with the updated
        /// product details if successful; otherwise, an <see cref="ErrorResponseDto"/> describing the error.</returns>
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
        /// Deletes the product with the specified identifier.
        /// </summary>
        /// <remarks>Requires authorization. Returns a 200 response if the product is successfully
        /// deleted. If the product does not exist, a 404 response is returned. If the caller is unauthorized or
        /// forbidden, a 401 or 403 response is returned, respectively.</remarks>
        /// <param name="id">The unique identifier of the product to delete.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the delete operation.</returns>
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
