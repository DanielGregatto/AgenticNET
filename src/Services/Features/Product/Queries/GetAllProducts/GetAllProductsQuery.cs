using Domain.Contracts.Common;
using Services.Contracts.Results;
using MediatR;
using System.Collections.Generic;

namespace Services.Features.Product.Queries.GetAllProducts
{
    public class GetAllProductsQuery : IRequest<Result<IEnumerable<ProductResult>>>
    {
    }
}
