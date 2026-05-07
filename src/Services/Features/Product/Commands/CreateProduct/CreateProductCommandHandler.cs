using Data.Context;
using Domain.Contracts.Common;
using Domain.Enums;
using Domain.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Services.Contracts.Results;
using Services.Core;
using System.Threading;
using System.Threading.Tasks;

namespace Services.Features.Product.Commands.CreateProduct
{
    public class CreateProductCommandHandler : BaseCommandHandler,
        IRequestHandler<CreateProductCommand, Result<ProductResult>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidator<CreateProductCommand> _validator;
        private readonly ILogger<CreateProductCommandHandler> _logger;
        private readonly IStringLocalizer _localizer;

        public CreateProductCommandHandler(
            AppDbContext context,
            IUser user,
            IUnitOfWork unitOfWork,
            IValidator<CreateProductCommand> validator,
            ILogger<CreateProductCommandHandler> logger,
            IStringLocalizer<Domain.Resources.Messages> localizer)
            : base(context, user)
        {
            _unitOfWork = unitOfWork;
            _validator = validator;
            _logger = logger;
            _localizer = localizer;
        }

        public async Task<Result<ProductResult>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Executing CreateProductCommand for product: {ProductName}, Slug: {Slug}",
                request.Name, request.Slug);

            var validationError = await ValidateAsync<CreateProductCommand, ProductResult>(_validator, request, cancellationToken);
            if (validationError != null)
            {
                _logger.LogWarning("Validation failed for CreateProductCommand. Product: {ProductName}",
                    request.Name);
                return validationError;
            }

            // TODO: Implement Azure Blob Storage service to upload request.Image and get the stored filename
            string imageFileName = null;
            if (request.Image != null && request.Image.Length > 0)
            {
                // TODO: imageFileName = await _blobStorageService.UploadAsync(request.Image, cancellationToken);
            }

            var product = new Domain.Product
            {
                Name = request.Name,
                Description = request.Description,
                Slug = request.Slug,
                Price = request.Price,
                Sku = request.Sku,
                Stock = request.Stock,
                ImageFileName = imageFileName,
                Active = request.Active
            };

            await _context.Products.AddAsync(product, cancellationToken);

            var saved = await _unitOfWork.CommitAsync(cancellationToken);
            if (saved == 0)
            {
                _logger.LogError("Failed to save product to database. Product: {ProductName}",
                    request.Name);
                return Result<ProductResult>.Failure(_localizer["DatabaseError"], ErrorTypes.Database);
            }

            _logger.LogInformation("Successfully created product {ProductId} with name: {ProductName}",
                product.Id, product.Name);

            return Result<ProductResult>.Success(ProductResult.FromEntity(product));
        }
    }
}
