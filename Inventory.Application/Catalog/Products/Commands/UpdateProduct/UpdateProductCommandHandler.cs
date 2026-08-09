using Inventory.Domain;
using Inventory.Domain.Catalog.Products.Entities;
using Inventory.Domain.Catalog.Products.Errors;
using POS.Shared.Application.Messaging;
using POS.Shared.Domain;

namespace Inventory.Application.Catalog.Products.Commands.UpdateProduct
{
    internal sealed class UpdateProductCommandHandler : ICommandHandler<UpdateProductCommand>
    {
        private readonly IInventoryUnitOfWork _unitOfWork;

        public UpdateProductCommandHandler(IInventoryUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
        {
            var product = await _unitOfWork.ProductRepository.GetByIdAsync(request.Id, cancellationToken);
            if (product is null)
                return Result.Failure(ProductErrors.NotFound(request.Id));

            var updateResult = product.Update(
                request.Barcode, request.NameAr, request.NameEn, request.Description,
                request.CategoryId, request.UnitId, request.SupplierId,
                request.PurchasePrice, request.SellingPrice, request.WholesalePrice,
                request.ReorderLevel, request.MaxStockLevel,
                request.IsWeighable, request.IsActive, request.TrackExpiry,
                request.TaxRate, request.ImageUrl);

            if (updateResult.IsFailure)
                return updateResult;

            _unitOfWork.ProductRepository.Update(product);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
