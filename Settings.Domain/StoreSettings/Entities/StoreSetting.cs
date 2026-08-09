using POS.Shared.Domain;

namespace Settings.Domain.StoreSettings.Entities
{
    public sealed class StoreSetting : Entity
    {
        public string StoreName { get; private set; } = default!;
        public string? Address { get; private set; }
        public string? Phone { get; private set; }
        public decimal TaxRate { get; private set; }
        public bool IsTaxIncluded { get; private set; }
        public string Currency { get; private set; } = default!;
        public string? InvoiceFooterMessage { get; private set; }
        public bool AllowNegativeStock { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        private StoreSetting() { } // EF Core

        private StoreSetting(Guid id, string storeName, string? address, string? phone,
            decimal taxRate, bool isTaxIncluded, string currency,
            string? invoiceFooterMessage, bool allowNegativeStock)
            : base(id)
        {
            StoreName = storeName;
            Address = address;
            Phone = phone;
            TaxRate = taxRate;
            IsTaxIncluded = isTaxIncluded;
            Currency = currency;
            InvoiceFooterMessage = invoiceFooterMessage;
            AllowNegativeStock = allowNegativeStock;
            UpdatedAt = DateTime.UtcNow;
        }

        public static Result<StoreSetting> Create(
            string storeName, string currency,
            decimal taxRate = 0, bool isTaxIncluded = true,
            string? address = null, string? phone = null,
            string? invoiceFooterMessage = null, bool allowNegativeStock = false)
        {
            if (string.IsNullOrWhiteSpace(storeName))
                return Result<StoreSetting>.Failure(StoreSettingErrors.StoreNameRequired);

            if (string.IsNullOrWhiteSpace(currency))
                return Result<StoreSetting>.Failure(StoreSettingErrors.CurrencyRequired);

            if (taxRate < 0 || taxRate > 100)
                return Result<StoreSetting>.Failure(StoreSettingErrors.TaxRateInvalid);

            var setting = new StoreSetting(
                Guid.NewGuid(), storeName.Trim(), address?.Trim(), phone?.Trim(),
                taxRate, isTaxIncluded, currency.Trim().ToUpperInvariant(),
                invoiceFooterMessage?.Trim(), allowNegativeStock);

            return Result<StoreSetting>.Success(setting);
        }

        public Result Update(
            string storeName, string? address, string? phone,
            decimal taxRate, bool isTaxIncluded, string currency,
            string? invoiceFooterMessage, bool allowNegativeStock)
        {
            if (string.IsNullOrWhiteSpace(storeName))
                return Result.Failure(StoreSettingErrors.StoreNameRequired);

            if (string.IsNullOrWhiteSpace(currency))
                return Result.Failure(StoreSettingErrors.CurrencyRequired);

            if (taxRate < 0 || taxRate > 100)
                return Result.Failure(StoreSettingErrors.TaxRateInvalid);

            StoreName = storeName.Trim();
            Address = address?.Trim();
            Phone = phone?.Trim();
            TaxRate = taxRate;
            IsTaxIncluded = isTaxIncluded;
            Currency = currency.Trim().ToUpperInvariant();
            InvoiceFooterMessage = invoiceFooterMessage?.Trim();
            AllowNegativeStock = allowNegativeStock;
            UpdatedAt = DateTime.UtcNow;

            return Result.Success();
        }
    }
}
