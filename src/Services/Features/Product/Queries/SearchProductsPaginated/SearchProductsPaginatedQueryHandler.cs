using Data.Context;
using Domain.Contracts.API;
using Domain.Contracts.Common;
using Services.Contracts.Results;
using Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Services.Core;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Services.Features.Product.Queries.SearchProductsPaginated
{
    public class SearchProductsPaginatedQueryHandler : BaseQueryHandler,
        IRequestHandler<SearchProductsPaginatedQuery, Result<PaginatedResponseDto<ProductResult>>>
    {
        public SearchProductsPaginatedQueryHandler(
            AppDbContext context,
            IUser user) : base(context, user)
        {
        }

        public async Task<Result<PaginatedResponseDto<ProductResult>>> Handle(
            SearchProductsPaginatedQuery request,
            CancellationToken cancellationToken)
        {
            var query = _context.Products
                .Where(x => !x.Deleted)
                .AsNoTracking();

            if (request.Predicate != null)
                query = query.Where(request.Predicate);

            if (!string.IsNullOrEmpty(request.OrderByProperty))
            {
                query = request.OrderByDescending
                    ? query.OrderByDescending(x => EF.Property<object>(x, request.OrderByProperty))
                    : query.OrderBy(x => EF.Property<object>(x, request.OrderByProperty));
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var productDtos = items.Select(ProductResult.FromEntity).ToList();

            var paginatedResponse = new PaginatedResponseDto<ProductResult>
            {
                PageIndex = request.PageIndex,
                PageSize = request.PageSize,
                TotalCount = totalCount,
                HasPreviousPage = request.PageIndex > 1,
                HasNextPage = request.PageIndex * request.PageSize < totalCount,
                Items = productDtos
            };

            return Result<PaginatedResponseDto<ProductResult>>.Success(paginatedResponse);
        }
    }
}
