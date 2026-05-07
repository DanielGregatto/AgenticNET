using Data.Context;
using Domain.Contracts.Common;
using Services.Contracts.Results;
using Domain.Enums;
using Domain.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Services.Core;
using System.Threading;
using System.Threading.Tasks;

namespace Services.Features.Product.Commands.UpdateProduct
{
    public class UpdateProductCommandHandler : BaseCommandHandler,
        IRequestHandler<UpdateProductCommand, Result<ProductResult>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidator<UpdateProductCommand> _validator;
        private readonly IStringLocalizer<Domain.Resources.Messages> _localizer;

        public UpdateProductCommandHandler(
            AppDbContext context,
            IUser user,
            IUnitOfWork unitOfWork,
            IValidator<UpdateProductCommand> validator,
            IStringLocalizer<Domain.Resources.Messages> localizer)
            : base(context, user)
        {
            _unitOfWork = unitOfWork;
            _validator = validator;
            _localizer = localizer;
        }

        public async Task<Result<ProductResult>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
        {
            var validationError = await ValidateAsync<UpdateProductCommand, ProductResult>(_validator, request, cancellationToken);
            if (validationError != null)
                return validationError;

            var product = await _context.Products
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.Deleted, cancellationToken);

            if (product == null)
                return Result<ProductResult>.NotFound(_localizer["Product_NotFound"]);

            // TODO: Implement Azure Blob Storage service to upload request.Image and get the stored filename
            if (request.Image != null && request.Image.Length > 0)
            {
                // TODO: product.ImageFileName = await _blobStorageService.UploadAsync(request.Image, cancellationToken);
            }

            product.Name = request.Name;
            product.Description = request.Description;
            product.Slug = request.Slug;
            product.Price = request.Price;
            product.Sku = request.Sku;
            product.Stock = request.Stock;
            product.Active = request.Active;

            _context.Products.Update(product);

            var saved = await _unitOfWork.CommitAsync(cancellationToken);
            if (saved == 0)
                return Result<ProductResult>.Failure(_localizer["Product_UpdateError"], ErrorTypes.Database);

            return Result<ProductResult>.Success(ProductResult.FromEntity(product));
        }
    }
}
