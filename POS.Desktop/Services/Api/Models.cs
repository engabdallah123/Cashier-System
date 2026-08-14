namespace POS.Desktop.Services.Api
{
    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        public LoginRequest() { }
        public LoginRequest(string username, string password)
        {
            Username = username;
            Password = password;
        }
    }

    public record AuthResponse(string AccessToken, string RefreshToken, DateTime Expiration, string UserId, string FullName, string Role);

    public record ProductDto(
        Guid Id,
        string Barcode,
        string NameAr,
        string NameEn,
        string? Description,
        Guid CategoryId,
        string? CategoryName,
        Guid UnitId,
        string? UnitSymbol,
        Guid? SupplierId,
        decimal PurchasePrice,
        decimal SellingPrice,
        decimal WholesalePrice,
        decimal QuantityInStock,
        decimal ReorderLevel,
        decimal MaxStockLevel,
        bool IsWeighable,
        bool IsActive,
        bool TrackExpiry,
        decimal TaxRate,
        string? ImageUrl);

    public class ProductImportResultDto
    {
        public int TotalRows { get; set; }
        public int SuccessCount { get; set; }
        public int ErrorCount { get; set; }
        public List<ProductImportErrorDto> Errors { get; set; } = new();
    }

    public class ProductImportErrorDto
    {
        public int RowNumber { get; set; }
        public string Barcode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
    }

    public record CategoryDto(
        Guid Id,
        string NameAr,
        string NameEn,
        Guid? ParentCategoryId,
        bool IsActive = true,
        DateTime CreatedAt = default);

    public record CreateCategoryRequest(string NameAr, string NameEn, Guid? ParentCategoryId = null);
    public record UpdateCategoryRequest(Guid Id, string NameAr, string NameEn, Guid? ParentCategoryId);

    public record UpdateProductCommandModel(
        Guid Id,
        string Barcode,
        string NameAr,
        string NameEn,
        string? Description,
        Guid CategoryId,
        Guid UnitId,
        Guid? SupplierId,
        decimal PurchasePrice,
        decimal SellingPrice,
        decimal WholesalePrice,
        decimal ReorderLevel,
        decimal MaxStockLevel,
        bool IsWeighable,
        bool IsActive,
        bool TrackExpiry,
        decimal TaxRate,
        string? ImageUrl);

    public class CreateProductFormModel
    {
        public string Barcode { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid CategoryId { get; set; }
        public Guid UnitId { get; set; }
        public Guid? SupplierId { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal SellingPrice { get; set; }
        public decimal WholesalePrice { get; set; }
        public decimal InitialStock { get; set; } = 0;
        public decimal ReorderLevel { get; set; } = 5;
        public decimal MaxStockLevel { get; set; } = 100;
        public bool IsWeighable { get; set; }
        public bool IsActive { get; set; } = true;
        public bool TrackExpiry { get; set; }
        public decimal TaxRate { get; set; }
    }
    public record UnitDto(Guid Id, string NameAr, string NameEn, string Symbol);
    public record SupplierDto(Guid Id, string Name, string Phone, string? Email, string? Address, string? ContactPerson);
    public record CustomerDto(Guid Id, string Name, string Phone, string? Email, string? Address, int LoyaltyPoints, decimal Balance);
    public record CreateCustomerRequest(string Name, string Phone, string? Email = null, string? Address = null);

    public record CreateSaleItemRequest(Guid ProductId, decimal Quantity, decimal UnitPrice, decimal Discount = 0, decimal Tax = 0);
    public record CreateSaleCommand(
        Guid CashierId,
        Guid ShiftId,
        List<CreateSaleItemRequest> Items,
        Guid? CustomerId = null,
        decimal DiscountAmount = 0,
        decimal TaxAmount = 0,
        decimal PaidAmount = 0,
        string PaymentMethod = "Cash",
        string? Notes = null);

    public record OpenShiftCommand(Guid CashierId, decimal OpeningCash, string? Notes = null);
    public record CloseShiftCommand(Guid ShiftId, decimal ActualClosingCash, string? ClosingNotes = null);
    public record ShiftDto(
        Guid Id,
        Guid CashierId,
        string? CashierName,
        DateTime OpenedAt,
        DateTime? ClosedAt,
        decimal OpeningCash,
        decimal ClosingCash,
        decimal SystemCash,
        decimal CashDifference,
        string Status,
        decimal TotalSales = 0,
        decimal TotalCash = 0,
        decimal TotalCard = 0,
        decimal TotalWallet = 0,
        decimal TotalCredit = 0,
        decimal TotalDiscount = 0,
        decimal TotalTax = 0,
        int TotalInvoices = 0,
        int TotalReturns = 0,
        string? Notes = null,
        string? ClosingNotes = null);

    public record PeriodMetricsDto(
        decimal TotalSales,
        decimal NetProfit,
        int TotalInvoices,
        decimal TotalPurchases,
        decimal TotalExpenses);

    public record PaymentMethodSummaryDto(
        string PaymentMethod,
        decimal TotalAmount,
        int InvoiceCount,
        decimal Percentage);

    public record LowStockProductDto(
        Guid ProductId,
        string ProductName,
        string Barcode,
        decimal QuantityInStock,
        decimal ReorderLevel);

    public record CashierPerformanceDto(
        Guid CashierId,
        string CashierName,
        int TotalShifts,
        int TotalInvoices,
        decimal TotalSalesAmount,
        decimal TotalCashDifference);

    public record DashboardDataDto(
        decimal TotalSales,
        int TotalInvoices,
        decimal TotalPurchases,
        decimal TotalExpenses,
        decimal NetProfit,
        decimal TotalSalesReturns,
        decimal TotalPurchaseReturns,
        decimal AverageInvoiceValue,
        decimal ProfitMarginPercentage,
        int LowStockProductsCount,
        PeriodMetricsDto? TodayMetrics,
        PeriodMetricsDto? MonthMetrics,
        PeriodMetricsDto? YearMetrics,
        List<TopProductDto>? TopSellingProducts,
        List<CashierPerformanceDto>? CashierPerformances,
        List<PaymentMethodSummaryDto>? PaymentMethodsSummary,
        List<LowStockProductDto>? LowStockProductsList)
    {
        public int LowStockCount => LowStockProductsCount;
    }

    public record TopProductDto(
        Guid ProductId,
        string? ProductName,
        string? Barcode,
        decimal TotalQuantitySold,
        decimal TotalRevenue)
    {
        public string Name => !string.IsNullOrWhiteSpace(ProductName) ? ProductName : "منتج بدون اسم";
    }

    public record ExpenseDto(Guid Id, string Title, string? Description, string? Category, decimal Amount, DateTime ExpenseDate, string? Notes)
    {
        public string DisplayCategory => !string.IsNullOrWhiteSpace(Category)
            ? Category
            : (!string.IsNullOrWhiteSpace(Description) ? Description : "عام");
    }
    public record CreateExpenseRequest(string Title, decimal Amount, Guid CreatedByUserId, string? Description = null, DateTime? ExpenseDate = null, string? Notes = null);

    public record CreateSupplierRequest(string Name, string Phone, string? Email = null, string? Address = null, string? ContactPerson = null);

    public record PurchaseItemDto(Guid Id, Guid ProductId, string? ProductName, decimal Quantity, decimal UnitCost, decimal Discount, decimal Tax, decimal Total, DateTime? ExpiryDate, string? BatchNumber)
    {
        public decimal UnitCostPrice => UnitCost;
        public decimal TotalCost => Total;
    }
    public record PurchaseDto(Guid Id, string InvoiceNumber, DateTime PurchaseDate, Guid SupplierId, string? SupplierName, decimal TotalAmount, decimal PaidAmount, decimal RemainingAmount, string Status, string? Notes, List<PurchaseItemDto>? Items);
    public record CreatePurchaseItemRequest(Guid ProductId, decimal Quantity, decimal UnitCost, decimal Discount = 0, decimal Tax = 0, DateTime? ExpiryDate = null, string? BatchNumber = null)
    {
        public decimal UnitCostPrice => UnitCost;
    }
    public record CreatePurchaseRequest(string InvoiceNumber, Guid SupplierId, Guid CreatedByUserId, List<CreatePurchaseItemRequest> Items, string? InternalNumber = null, decimal DiscountAmount = 0, decimal TaxAmount = 0, decimal PaidAmount = 0, int PaymentMethod = 1, string? Notes = null);

    public record SaleItemDto(Guid Id, Guid ProductId, string? ProductName, string? Barcode, decimal Quantity, decimal UnitPrice, decimal Discount, decimal Tax, decimal Total);
    public record SaleDto(Guid Id, string InvoiceNumber, DateTime SaleDate, Guid CashierId, string? CashierName, Guid? CustomerId, string? CustomerName, Guid ShiftId, decimal SubTotal, decimal DiscountAmount, decimal TaxAmount, decimal TotalAmount, decimal PaidAmount, decimal ChangeAmount, string PaymentMethod, string Status, string? Notes, List<SaleItemDto>? Items);

    public record StoreSettingDto(
        Guid Id,
        string StoreName,
        string? Address,
        string? Phone,
        decimal TaxRate,
        bool IsTaxIncluded,
        string Currency,
        string? InvoiceFooterMessage,
        bool AllowNegativeStock,
        DateTime UpdatedAt);

    public record UpdateStoreSettingRequest(
        string StoreName,
        string? Address,
        string? Phone,
        decimal TaxRate,
        bool IsTaxIncluded,
        string Currency,
        string? InvoiceFooterMessage,
        bool AllowNegativeStock);

    public record AuditLogDto(
        Guid Id,
        Guid? UserId,
        string Action,
        string EntityName,
        Guid? EntityId,
        string? OldValues,
        string? NewValues,
        string? IpAddress,
        DateTime CreatedAt);

    public record OnlineProductLookupResult(string Barcode, string? NameAr, string? NameEn, string? ImageUrl, byte[]? ImageBytes);
}
