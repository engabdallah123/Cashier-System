using Inventory.Domain;
using Inventory.Domain.Stock.StockBalances;
using Inventory.Domain.Stock.StockMovements;
using Inventory.Domain.Stock.StockTransfers;
using POS.Shared.Application.Messaging;
using POS.Shared.Domain;

namespace Inventory.Application.Stock.StockTransfers.Commands.ExecuteStockTransfer
{
    internal sealed class ExecuteStockTransferCommandHandler : ICommandHandler<ExecuteStockTransferCommand>
    {
        private readonly IInventoryUnitOfWork _unitOfWork;

        public ExecuteStockTransferCommandHandler(IInventoryUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(ExecuteStockTransferCommand request, CancellationToken cancellationToken)
        {
            var transfer = await _unitOfWork.StockTransferRepository.FindAsync(t => t.Id == request.TransferId, new[] { "Items" });
            if (transfer is null)
                return Result.Failure(StockTransferErrors.NotFound(request.TransferId));

            var executeResult = transfer.Execute();
            if (executeResult.IsFailure)
                return executeResult;

            // Apply items stock transfer
            foreach (var item in transfer.Items)
            {
                // 1. Deduct from Source Warehouse
                var sourceBalance = await _unitOfWork.StockBalanceRepository
                    .GetByProductAndWarehouseAsync(item.ProductId, transfer.SourceWarehouseId, cancellationToken);

                if (sourceBalance is null)
                    return Result.Failure(StockBalanceErrors.NotFound(item.ProductId, transfer.SourceWarehouseId));

                var beforeSrc = sourceBalance.QuantityOnHand;
                var decResult = sourceBalance.Decrease(item.Quantity);
                if (decResult.IsFailure)
                    return decResult;

                var afterSrc = sourceBalance.QuantityOnHand;

                // Log Source Stock Movement (TransferOut)
                var srcMovement = StockMovement.Create(
                    item.ProductId, transfer.SourceWarehouseId, StockMovementType.TransferOut,
                    -item.Quantity, beforeSrc, afterSrc, "StockTransfer", transfer.Id, request.ExecutedBy);

                if (srcMovement.IsFailure) return Result.Failure(srcMovement.Error);
                await _unitOfWork.StockMovementRepository.AddAsync(srcMovement.Value!);

                // 2. Add to Destination Warehouse
                var destBalance = await _unitOfWork.StockBalanceRepository
                    .GetByProductAndWarehouseAsync(item.ProductId, transfer.DestinationWarehouseId, cancellationToken);

                if (destBalance is null)
                {
                    var newBalanceRes = StockBalance.Create(item.ProductId, transfer.DestinationWarehouseId);
                    if (newBalanceRes.IsFailure) return Result.Failure(newBalanceRes.Error);
                    destBalance = newBalanceRes.Value!;
                    await _unitOfWork.StockBalanceRepository.AddAsync(destBalance);
                }

                var beforeDest = destBalance.QuantityOnHand;
                destBalance.Increase(item.Quantity);
                var afterDest = destBalance.QuantityOnHand;

                // Log Destination Stock Movement (TransferIn)
                var destMovement = StockMovement.Create(
                    item.ProductId, transfer.DestinationWarehouseId, StockMovementType.TransferIn,
                    item.Quantity, beforeDest, afterDest, "StockTransfer", transfer.Id, request.ExecutedBy);

                if (destMovement.IsFailure) return Result.Failure(destMovement.Error);
                await _unitOfWork.StockMovementRepository.AddAsync(destMovement.Value!);
            }

            _unitOfWork.StockTransferRepository.Update(transfer);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
