using Data.Context;
using Domain.Contracts.Common;
using Services.Contracts.Results;
using Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Services.Core;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Services.Features.Product.Queries.GetProductById
{
    public class GetProductByIdQueryHandler : BaseQueryHandler,
        IRequestHandler<GetProductByIdQuery, Result<ProductResult>>
    {
        private readonly ILogger<GetProductByIdQueryHandler> _logger;
        private readonly IStringLocalizer _localizer;

        public GetProductByIdQueryHandler(
            AppDbContext context,
            IUser user,
            ILogger<GetProductByIdQueryHandler> logger,
            IStringLocalizer<Domain.Resources.Messages> localizer)
            : base(context, user)
        {
            _logger = logger;
            _localizer = localizer;
        }

        public async Task<Result<ProductResult>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Querying product by ID: {ProductId}", request.Id);

            var product = await _context.Products
                .Where(x => !x.Deleted)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (product == null)
            {
                _logger.LogWarning("Product not found with ID: {ProductId}", request.Id);
                return Result<ProductResult>.NotFound(_localizer["NotFound"]);
            }

            _logger.LogInformation("Successfully retrieved product {ProductId} with name: {ProductName}",
                product.Id, product.Name);

            return Result<ProductResult>.Success(ProductResult.FromEntity(product));
        }
    }
}
