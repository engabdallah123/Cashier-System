using Inventory.Domain;
using Inventory.Domain.Pricing.PriceLists;
using POS.Shared.Application.Messaging;
using POS.Shared.Domain;

namespace Inventory.Application.Pricing.PriceLists.Commands.CreatePriceList
{
    internal sealed class CreatePriceListCommandHandler : ICommandHandler<CreatePriceListCommand, Guid>
    {
        private readonly IInventoryUnitOfWork _unitOfWork;

        public CreatePriceListCommandHandler(IInventoryUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Guid>> Handle(CreatePriceListCommand request, CancellationToken cancellationToken)
        {
            var priceListResult = PriceList.Create(request.Name, request.Description, request.IsDefault);
            if (priceListResult.IsFailure)
                return Result<Guid>.Failure(priceListResult.Error);

            var priceList = priceListResult.Value!;

            await _unitOfWork.PriceListRepository.AddAsync(priceList);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(priceList.Id);
        }
    }
}
