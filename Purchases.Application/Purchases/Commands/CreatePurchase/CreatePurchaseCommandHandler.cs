using POS.Shared.Application.Messaging;
using POS.Shared.Domain;
using Purchases.Domain;
using Purchases.Domain.Purchases.Entities;

namespace Purchases.Application.Purchases.Commands.CreatePurchase
{
    internal sealed class CreatePurchaseCommandHandler : ICommandHandler<CreatePurchaseCommand, Guid>
    {
        private readonly IPurchasesUnitOfWork _unitOfWork;

        public CreatePurchaseCommandHandler(IPurchasesUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
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

            await _unitOfWork.PurchaseRepository.AddAsync(purchase);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(purchase.Id);
        }
    }
}
