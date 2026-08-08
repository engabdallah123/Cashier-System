using Inventory.Domain;
using Inventory.Domain.Stock.StockTransfers;
using POS.Shared.Application.Messaging;
using POS.Shared.Domain;

namespace Inventory.Application.Stock.StockTransfers.Commands.CreateStockTransfer
{
    internal sealed class CreateStockTransferCommandHandler : ICommandHandler<CreateStockTransferCommand, Guid>
    {
        private readonly IInventoryUnitOfWork _unitOfWork;

        public CreateStockTransferCommandHandler(IInventoryUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Guid>> Handle(CreateStockTransferCommand request, CancellationToken cancellationToken)
        {
            var transferResult = StockTransfer.Create(
                request.TransferNumber,
                request.SourceWarehouseId,
                request.DestinationWarehouseId,
                request.CreatedBy,
                request.Notes);

            if (transferResult.IsFailure)
                return Result<Guid>.Failure(transferResult.Error);

            var transfer = transferResult.Value!;

            foreach (var item in request.Items)
            {
                var addItemResult = transfer.AddItem(item.ProductId, item.Quantity);
                if (addItemResult.IsFailure)
                    return Result<Guid>.Failure(addItemResult.Error);
            }

            await _unitOfWork.StockTransferRepository.AddAsync(transfer);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(transfer.Id);
        }
    }
}
