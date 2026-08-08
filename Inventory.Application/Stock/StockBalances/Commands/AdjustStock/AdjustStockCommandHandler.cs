using Inventory.Domain;
using Inventory.Domain.Catalog.Products.Errors;
using Inventory.Domain.Stock.StockBalances;
using Inventory.Domain.Stock.StockMovements;
using Inventory.Domain.Stock.Warehouses;
using POS.Shared.Application.Messaging;
using POS.Shared.Domain;

namespace Inventory.Application.Stock.StockBalances.Commands.AdjustStock
{
    internal sealed class AdjustStockCommandHandler : ICommandHandler<AdjustStockCommand>
    {
        private readonly IInventoryUnitOfWork _unitOfWork;

        public AdjustStockCommandHandler(IInventoryUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(AdjustStockCommand request, CancellationToken cancellationToken)
        {
            var product = await _unitOfWork.ProductRepository.GetByIdAsync(request.ProductId, cancellationToken);
            if (product is null)
                return Result.Failure(ProductErrors.NotFound(request.ProductId));

            var warehouse = await _unitOfWork.WarehouseRepository.FindAsync(w => w.Id == request.WarehouseId);
            if (warehouse is null)
                return Result.Failure(WarehouseErrors.NotFound(request.WarehouseId));

            // Get or create stock balance for product + warehouse
            var balance = await _unitOfWork.StockBalanceRepository
                .GetByProductAndWarehouseAsync(request.ProductId, request.WarehouseId, cancellationToken);

            if (balance is null)
            {
                var balanceResult = StockBalance.Create(request.ProductId, request.WarehouseId);
                if (balanceResult.IsFailure)
                    return Result.Failure(balanceResult.Error);

                balance = balanceResult.Value!;
                await _unitOfWork.StockBalanceRepository.AddAsync(balance);
            }

            var beforeQuantity = balance.QuantityOnHand;

            // Apply adjustment to StockBalance
            if (request.Quantity > 0)
            {
                balance.Increase(request.Quantity);
            }
            else if (request.Quantity < 0)
            {
                var decreaseResult = balance.Decrease(Math.Abs(request.Quantity));
                if (decreaseResult.IsFailure)
                    return decreaseResult;
            }

            var afterQuantity = balance.QuantityOnHand;

            // Also update total product quantity on hand
            var adjustProductResult = product.AdjustStock(request.Quantity);
            if (adjustProductResult.IsFailure)
                return adjustProductResult;

            _unitOfWork.ProductRepository.Update(product);

            // Record StockMovement audit log
            var movementResult = StockMovement.Create(
                request.ProductId,
                request.WarehouseId,
                request.MovementType,
                request.Quantity,
                beforeQuantity,
                afterQuantity,
                request.ReferenceType,
                request.ReferenceId,
                request.PerformedBy);

            if (movementResult.IsFailure)
                return Result.Failure(movementResult.Error);

            await _unitOfWork.StockMovementRepository.AddAsync(movementResult.Value!);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
