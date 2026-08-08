using Inventory.Domain;
using Inventory.Domain.Catalog.ProductBarcodes;
using Inventory.Domain.Catalog.Products.Errors;
using POS.Shared.Application.Messaging;
using POS.Shared.Domain;

namespace Inventory.Application.Catalog.Products.Commands.AddProductBarcode
{
    internal sealed class AddProductBarcodeCommandHandler : ICommandHandler<AddProductBarcodeCommand, Guid>
    {
        private readonly IInventoryUnitOfWork _unitOfWork;

        public AddProductBarcodeCommandHandler(IInventoryUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Guid>> Handle(AddProductBarcodeCommand request, CancellationToken cancellationToken)
        {
            var product = await _unitOfWork.ProductRepository.GetByIdAsync(request.ProductId, cancellationToken);
            if (product is null)
                return Result<Guid>.Failure(ProductErrors.NotFound(request.ProductId));

            var barcodeExists = await _unitOfWork.ProductRepository.BarcodeExistsAsync(request.Barcode, cancellationToken);
            if (barcodeExists)
                return Result<Guid>.Failure(ProductBarcodeErrors.DuplicateBarcode);

            var barcodeResult = ProductBarcode.Create(request.ProductId, request.Barcode, request.IsDefault);
            if (barcodeResult.IsFailure)
                return Result<Guid>.Failure(barcodeResult.Error);

            var barcodeEntity = barcodeResult.Value!;
            await _unitOfWork.ProductBarcodeRepository.AddAsync(barcodeEntity);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(barcodeEntity.Id);
        }
    }
}
