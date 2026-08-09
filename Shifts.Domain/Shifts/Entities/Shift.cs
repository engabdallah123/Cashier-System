using POS.Shared.Domain;
using POS.Shared.Domain.Events.Shifts;

namespace Shifts.Domain.Shifts.Entities
{
    public sealed class Shift : Entity
    {
        public Guid CashierId { get; private set; }
        public DateTime OpenedAt { get; private set; }
        public DateTime? ClosedAt { get; private set; }

        public decimal OpeningCash { get; private set; }
        public decimal ClosingCash { get; private set; }
        public decimal SystemCash { get; private set; }
        public decimal CashDifference { get; private set; }

        public ShiftStatus Status { get; private set; }

        public decimal TotalSales { get; private set; }
        public decimal TotalCash { get; private set; }
        public decimal TotalCard { get; private set; }
        public decimal TotalWallet { get; private set; }
        public decimal TotalCredit { get; private set; }

        public decimal TotalDiscount { get; private set; }
        public decimal TotalTax { get; private set; }
        public int TotalInvoices { get; private set; }
        public int TotalReturns { get; private set; }

        public string? Notes { get; private set; }
        public string? ClosingNotes { get; private set; }

        private Shift() { } // EF Core

        private Shift(Guid id, Guid cashierId, decimal openingCash, string? notes)
            : base(id)
        {
            CashierId = cashierId;
            OpenedAt = DateTime.UtcNow;
            OpeningCash = openingCash;
            Status = ShiftStatus.Open;
            Notes = notes;

            TotalSales = 0;
            TotalCash = openingCash;
            TotalCard = 0;
            TotalWallet = 0;
            TotalCredit = 0;
            TotalDiscount = 0;
            TotalTax = 0;
            TotalInvoices = 0;
            TotalReturns = 0;
            SystemCash = openingCash;
            ClosingCash = 0;
            CashDifference = 0;
        }

        public static Result<Shift> Open(Guid cashierId, decimal openingCash, string? notes = null)
        {
            if (cashierId == Guid.Empty)
                return Result<Shift>.Failure(ShiftErrors.CashierRequired);

            if (openingCash < 0)
                return Result<Shift>.Failure(ShiftErrors.InvalidOpeningCash);

            var shift = new Shift(Guid.NewGuid(), cashierId, openingCash, notes?.Trim());
            shift.RaiseDomainEvent(new ShiftOpenedIntegrationEvent(shift.Id, cashierId, openingCash));
            return Result<Shift>.Success(shift);
        }

        public Result Close(decimal actualClosingCash, string? closingNotes = null)
        {
            if (Status != ShiftStatus.Open)
                return Result.Failure(ShiftErrors.NotOpen);

            if (actualClosingCash < 0)
                return Result.Failure(ShiftErrors.InvalidClosingCash);

            ClosedAt = DateTime.UtcNow;
            ClosingCash = actualClosingCash;
            SystemCash = TotalCash; // OpeningCash + Cash Sales - Cash Returns
            CashDifference = ClosingCash - SystemCash;
            Status = ShiftStatus.Closed;
            ClosingNotes = closingNotes?.Trim();

            RaiseDomainEvent(new ShiftClosedIntegrationEvent(Id, CashierId, SystemCash, CashDifference));
            return Result.Success();
        }

        public Result RecordSale(decimal totalAmount, decimal discountAmount, decimal taxAmount, string paymentMethod)
        {
            if (Status != ShiftStatus.Open)
                return Result.Failure(ShiftErrors.NotOpen);

            TotalSales += totalAmount;
            TotalDiscount += discountAmount;
            TotalTax += taxAmount;
            TotalInvoices++;

            switch (paymentMethod.Trim().ToLowerInvariant())
            {
                case "cash":
                    TotalCash += totalAmount;
                    break;
                case "card":
                    TotalCard += totalAmount;
                    break;
                case "mobilewallet":
                case "wallet":
                    TotalWallet += totalAmount;
                    break;
                case "credit":
                    TotalCredit += totalAmount;
                    break;
                default:
                    TotalCash += totalAmount;
                    break;
            }

            SystemCash = TotalCash;
            return Result.Success();
        }

        public Result RecordReturn(decimal returnAmount, string refundMethod)
        {
            if (Status != ShiftStatus.Open)
                return Result.Failure(ShiftErrors.NotOpen);

            TotalReturns++;
            if (refundMethod.Equals("cash", StringComparison.OrdinalIgnoreCase))
            {
                TotalCash -= returnAmount;
            }

            SystemCash = TotalCash;
            return Result.Success();
        }

        public Result Cancel()
        {
            if (Status != ShiftStatus.Open)
                return Result.Failure(ShiftErrors.NotOpen);

            Status = ShiftStatus.Cancelled;
            ClosedAt = DateTime.UtcNow;
            return Result.Success();
        }
    }
}
