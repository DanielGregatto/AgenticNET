using Data.Context;
using Domain.Contracts.Common;
using Services.Contracts.Results;
using Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Services.Core;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Services.Features.Product.Queries.GetAllProducts
{
    public class GetAllProductsQueryHandler : BaseQueryHandler,
        IRequestHandler<GetAllProductsQuery, Result<IEnumerable<ProductResult>>>
    {
        public GetAllProductsQueryHandler(AppDbContext context, IUser user)
            : base(context, user)
        {
        }

        public async Task<Result<IEnumerable<ProductResult>>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
        {
            var products = await _context.Products
                .Where(x => !x.Deleted)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var productDtos = products.Select(ProductResult.FromEntity).ToList();

            return Result<IEnumerable<ProductResult>>.Success(productDtos);
        }
    }
}
