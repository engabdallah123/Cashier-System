using Inventory.Domain;
using Inventory.Domain.Catalog.Products.Entities;
using Inventory.Domain.Catalog.Products.Errors;
using POS.Shared.Application.Messaging;
using POS.Shared.Domain;

namespace Inventory.Application.Catalog.Products.Commands.CreateProduct
{
    internal sealed class CreateProductCommandHandler : ICommandHandler<CreateProductCommand, Guid>
    {
        private readonly IInventoryUnitOfWork _unitOfWork;

        public CreateProductCommandHandler(IInventoryUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Guid>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            var skuExists = await _unitOfWork.ProductRepository
                .SkuExistsAsync(request.Sku.Trim().ToUpperInvariant(), cancellationToken);

            if (skuExists)
                return Result<Guid>.Failure(ProductErrors.DuplicateSku);

            var skuResult = Sku.Create(request.Sku);
            if (skuResult.IsFailure)
                return Result<Guid>.Failure(skuResult.Error);

            var priceResult = Money.Create(request.Price, request.Currency);
            if (priceResult.IsFailure)
                return Result<Guid>.Failure(priceResult.Error);

            var productResult = Product.Create(
                request.Name,
                skuResult.Value!,
                priceResult.Value!,
                request.LowStockThreshold,
                categoryId: request.CategoryId,
                brandId: request.BrandId,
                unitId: request.UnitId);

            if (productResult.IsFailure)
                return Result<Guid>.Failure(productResult.Error);

            var product = productResult.Value!;

            await _unitOfWork.ProductRepository.AddAsync(product, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(product.Id);
        }
    }
}
