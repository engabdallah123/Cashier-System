using Inventory.Domain;
using Inventory.Domain.Stock.StockMovements;
using POS.Shared.Application.Messaging;
using POS.Shared.Domain;
using Purchases.Domain;
using Purchases.Domain.Purchases.Entities;

namespace Purchases.Application.Purchases.Commands.CreatePurchase
{
    internal sealed class CreatePurchaseCommandHandler : ICommandHandler<CreatePurchaseCommand, Guid>
    {
        private readonly IPurchasesUnitOfWork _purchasesUnitOfWork;
        private readonly IInventoryUnitOfWork _inventoryUnitOfWork;

        public CreatePurchaseCommandHandler(
            IPurchasesUnitOfWork purchasesUnitOfWork,
            IInventoryUnitOfWork inventoryUnitOfWork)
        {
            _purchasesUnitOfWork = purchasesUnitOfWork;
            _inventoryUnitOfWork = inventoryUnitOfWork;
        }

        public async Task<Result<Guid>> Handle(CreatePurchaseCommand request, CancellationToken cancellationToken)
        {
            var purchaseResult = Purchase.Create(
                request.InvoiceNumber, request.SupplierId, request.CreatedByUserId,
                request.InternalNumber, request.DiscountAmount, request.TaxAmount,
                request.PaidAmount, (PaymentMethod)request.PaymentMethod, request.Notes);

            if (purchaseResult.IsFailure)
                return Result<Guid>.Failure(purchaseResult.Error);

            var purchase = purchaseResult.Value!;

            foreach (var itemReq in request.Items)
            {
                var itemResult = purchase.AddItem(
                    itemReq.ProductId, itemReq.Quantity, itemReq.UnitCost,
                    itemReq.Discount, itemReq.Tax, itemReq.ExpiryDate, itemReq.BatchNumber);

                if (itemResult.IsFailure)
                    return Result<Guid>.Failure(itemResult.Error);
            }

            var receiveResult = purchase.Receive();
            if (receiveResult.IsFailure)
                return Result<Guid>.Failure(receiveResult.Error);

            await _purchasesUnitOfWork.PurchaseRepository.AddAsync(purchase);

            // زيادة رصيد المخزون وتسجيل حركة المخزون لكل عنصر في الفاتورة
            foreach (var item in purchase.Items)
            {
                var product = await _inventoryUnitOfWork.ProductRepository.GetByIdAsync(item.ProductId, cancellationToken);
                if (product is not null)
                {
                    product.AdjustStock(item.Quantity, allowNegativeStock: true);
                    _inventoryUnitOfWork.ProductRepository.Update(product);

                    var movementResult = StockMovement.Create(
                        item.ProductId,
                        item.Quantity,
                        StockMovementType.Purchase,
                        request.CreatedByUserId,
                        reference: purchase.InvoiceNumber,
                        notes: $"فاتورة شراء - رقم {purchase.InvoiceNumber}");

                    if (movementResult.IsSuccess)
                    {
                        await _inventoryUnitOfWork.StockMovementRepository.AddAsync(movementResult.Value!);
                    }
                }
            }

            await _purchasesUnitOfWork.SaveChangesAsync(cancellationToken);
            await _inventoryUnitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(purchase.Id);
        }
    }
}
