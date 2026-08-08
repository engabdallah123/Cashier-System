using Inventory.Domain;
using Inventory.Domain.Pricing.PriceLists;
using POS.Shared.Application.Messaging;
using POS.Shared.Domain;

namespace Inventory.Application.Pricing.PriceLists.Commands.DeletePriceList
{
    internal sealed class DeletePriceListCommandHandler : ICommandHandler<DeletePriceListCommand>
    {
        private readonly IInventoryUnitOfWork _unitOfWork;

        public DeletePriceListCommandHandler(IInventoryUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(DeletePriceListCommand request, CancellationToken cancellationToken)
        {
            var priceList = await _unitOfWork.PriceListRepository.FindAsync(pl => pl.Id == request.Id);
            if (priceList is null)
                return Result.Failure(PriceListErrors.NotFound(request.Id));

            _unitOfWork.PriceListRepository.Delete(priceList);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
