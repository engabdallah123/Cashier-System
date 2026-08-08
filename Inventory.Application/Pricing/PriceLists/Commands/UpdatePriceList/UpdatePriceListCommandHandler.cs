using Inventory.Domain;
using Inventory.Domain.Pricing.PriceLists;
using POS.Shared.Application.Messaging;
using POS.Shared.Domain;

namespace Inventory.Application.Pricing.PriceLists.Commands.UpdatePriceList
{
    internal sealed class UpdatePriceListCommandHandler : ICommandHandler<UpdatePriceListCommand>
    {
        private readonly IInventoryUnitOfWork _unitOfWork;

        public UpdatePriceListCommandHandler(IInventoryUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(UpdatePriceListCommand request, CancellationToken cancellationToken)
        {
            var priceList = await _unitOfWork.PriceListRepository.FindAsync(pl => pl.Id == request.Id);
            if (priceList is null)
                return Result.Failure(PriceListErrors.NotFound(request.Id));

            var updateRes = priceList.UpdateInfo(request.Name, request.Description, request.IsDefault);
            if (updateRes.IsFailure)
                return updateRes;

            _unitOfWork.PriceListRepository.Update(priceList);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
