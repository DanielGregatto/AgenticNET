using Data.Context;
using Domain.Contracts.Common;
using Domain.Enums;
using Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Services.Core;
using System.Threading;
using System.Threading.Tasks;

namespace Services.Features.Product.Commands.DeleteProduct
{
    public class DeleteProductCommandHandler : BaseCommandHandler,
        IRequestHandler<DeleteProductCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IStringLocalizer<Domain.Resources.Messages> _localizer;

        public DeleteProductCommandHandler(
            AppDbContext context,
            IUser user,
            IUnitOfWork unitOfWork,
            IStringLocalizer<Domain.Resources.Messages> localizer)
            : base(context, user)
        {
            _unitOfWork = unitOfWork;
            _localizer = localizer;
        }

        public async Task<Result> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.Deleted, cancellationToken);

            if (product == null)
                return Result.NotFound(_localizer["NotFound"]);

            // Soft delete
            product.Deleted = true;
            _context.Products.Update(product);

            var saved = await _unitOfWork.CommitAsync(cancellationToken);
            if (saved == 0)
                return Result.Failure(_localizer["DatabaseError"], ErrorTypes.Database);

            return Result.Success();
        }
    }
}
