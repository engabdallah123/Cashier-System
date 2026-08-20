using POS.Shared.Application.Messaging;
using POS.Shared.Domain;
using Sales.Domain;
using Sales.Domain.Sales;
using Shifts.Domain;

namespace Sales.Application.Sales.Commands.PaySaleInvoice;

internal sealed class PaySaleInvoiceCommandHandler : ICommandHandler<PaySaleInvoiceCommand>
{
    private readonly ISalesUnitOfWork _salesUnitOfWork;
    private readonly IShiftsUnitOfWork _shiftsUnitOfWork;

    public PaySaleInvoiceCommandHandler(ISalesUnitOfWork salesUnitOfWork, IShiftsUnitOfWork shiftsUnitOfWork)
    {
        _salesUnitOfWork = salesUnitOfWork;
        _shiftsUnitOfWork = shiftsUnitOfWork;
    }

    public async Task<Result> Handle(PaySaleInvoiceCommand request, CancellationToken cancellationToken)
    {
        var sale = await _salesUnitOfWork.SaleRepository.GetByIdAsync(request.SaleId);
        if (sale is null)
            return Result.Failure(SaleErrors.NotFound(request.SaleId));

        var paymentResult = sale.AddPayment(request.Amount);
        if (paymentResult.IsFailure)
            return paymentResult;

        _salesUnitOfWork.SaleRepository.Update(sale);

        // إذا كان هناك شفت مفتوح مرتبط بهذه الفاتورة أو شفت كاشير، نسجل تحصيل المديونية
        var shift = await _shiftsUnitOfWork.ShiftRepository.GetByIdAsync(sale.ShiftId);
        if (shift is not null && shift.Status == Shifts.Domain.Shifts.Entities.ShiftStatus.Open)
        {
            shift.RecordDebtCollection(request.Amount, "Cash");
            _shiftsUnitOfWork.ShiftRepository.Update(shift);
            await _shiftsUnitOfWork.SaveChangesAsync(cancellationToken);
        }

        await _salesUnitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
