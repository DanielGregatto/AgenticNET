using Domain.Contracts.Common;
using Services.Contracts.Results;
using MediatR;
using System;

namespace Services.Features.Product.Queries.GetProductById
{
    public class GetProductByIdQuery : IRequest<Result<ProductResult>>
    {
        public Guid Id { get; set; }

        public GetProductByIdQuery()
        {
        }

        public GetProductByIdQuery(Guid id)
        {
            Id = id;
        }
    }
}
