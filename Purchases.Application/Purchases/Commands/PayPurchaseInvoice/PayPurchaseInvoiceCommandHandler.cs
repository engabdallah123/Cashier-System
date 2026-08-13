using POS.Shared.Application.Messaging;
using POS.Shared.Domain;
using Purchases.Domain;
using Purchases.Domain.Purchases;

namespace Purchases.Application.Purchases.Commands.PayPurchaseInvoice;

internal sealed class PayPurchaseInvoiceCommandHandler : ICommandHandler<PayPurchaseInvoiceCommand>
{
    private readonly IPurchasesUnitOfWork _unitOfWork;

    public PayPurchaseInvoiceCommandHandler(IPurchasesUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(PayPurchaseInvoiceCommand request, CancellationToken cancellationToken)
    {
        var purchase = await _unitOfWork.PurchaseRepository.GetByIdAsync(request.PurchaseId);
        if (purchase is null)
            return Result.Failure(PurchaseErrors.NotFound(request.PurchaseId));

        var paymentResult = purchase.AddPayment(request.Amount);
        if (paymentResult.IsFailure)
            return paymentResult;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
