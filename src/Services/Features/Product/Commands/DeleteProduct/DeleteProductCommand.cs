using Domain.Contracts.Common;
using MediatR;
using System;

namespace Services.Features.Product.Commands.DeleteProduct
{
    public class DeleteProductCommand : IRequest<Result>
    {
        public Guid Id { get; set; }
    }
}
