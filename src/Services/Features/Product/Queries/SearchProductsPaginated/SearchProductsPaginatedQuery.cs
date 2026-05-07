using Services.Contracts.Results;
using MediatR;
using System;
using System.Linq.Expressions;
using Domain.Contracts.API;
using Domain.Contracts.Common;

namespace Services.Features.Product.Queries.SearchProductsPaginated
{
    public class SearchProductsPaginatedQuery : IRequest<Result<PaginatedResponseDto<ProductResult>>>
    {
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public Expression<Func<Domain.Product, bool>> Predicate { get; set; }
        public string OrderByProperty { get; set; }
        public bool OrderByDescending { get; set; } = false;
    }
}
