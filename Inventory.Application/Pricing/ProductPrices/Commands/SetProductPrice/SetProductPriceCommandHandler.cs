using Inventory.Domain;
using Inventory.Domain.Catalog.Products.Errors;
using Inventory.Domain.Pricing.PriceLists;
using Inventory.Domain.Pricing.ProductPrices;
using POS.Shared.Application.Messaging;
using POS.Shared.Domain;

namespace Inventory.Application.Pricing.ProductPrices.Commands.SetProductPrice
{
    internal sealed class SetProductPriceCommandHandler : ICommandHandler<SetProductPriceCommand, Guid>
    {
        private readonly IInventoryUnitOfWork _unitOfWork;

        public SetProductPriceCommandHandler(IInventoryUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Guid>> Handle(SetProductPriceCommand request, CancellationToken cancellationToken)
        {
            var product = await _unitOfWork.ProductRepository.GetByIdAsync(request.ProductId, cancellationToken);
            if (product is null)
                return Result<Guid>.Failure(ProductErrors.NotFound(request.ProductId));

            var priceList = await _unitOfWork.PriceListRepository.FindAsync(pl => pl.Id == request.PriceListId);
            if (priceList is null)
                return Result<Guid>.Failure(PriceListErrors.NotFound(request.PriceListId));

            var existingPrice = await _unitOfWork.ProductPriceRepository
                .FindAsync(pp => pp.ProductId == request.ProductId && pp.PriceListId == request.PriceListId);

            if (existingPrice is not null)
            {
                var updateRes = existingPrice.UpdatePrice(request.Price);
                if (updateRes.IsFailure)
                    return Result<Guid>.Failure(updateRes.Error);

                _unitOfWork.ProductPriceRepository.Update(existingPrice);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return Result<Guid>.Success(existingPrice.Id);
            }

            var priceResult = ProductPrice.Create(request.ProductId, request.PriceListId, request.Price, request.Currency);
            if (priceResult.IsFailure)
                return Result<Guid>.Failure(priceResult.Error);

            var newPrice = priceResult.Value!;
            await _unitOfWork.ProductPriceRepository.AddAsync(newPrice);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(newPrice.Id);
        }
    }
}
