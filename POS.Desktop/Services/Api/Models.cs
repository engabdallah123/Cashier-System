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

    public record AuthResponse(string Token, string RefreshToken, DateTime Expiration, string FullName, string Email, IList<string> Roles);

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

    public record CategoryDto(Guid Id, string NameAr, string NameEn, Guid? ParentCategoryId);
    public record UnitDto(Guid Id, string NameAr, string NameEn, string Symbol);
    public record SupplierDto(Guid Id, string Name, string Phone, string? Email, string? Address, string? ContactPerson);
    public record CustomerDto(Guid Id, string Name, string Phone, string? Email, string? Address, int LoyaltyPoints, decimal Balance);

    public record CreateSaleItemCommand(Guid ProductId, decimal Quantity, decimal UnitPrice, decimal Discount, decimal Tax);
    public record CreateSaleCommand(
        Guid CashierId,
        Guid? CustomerId,
        Guid ShiftId,
        decimal SubTotal,
        decimal DiscountAmount,
        decimal TaxAmount,
        decimal TotalAmount,
        decimal PaidAmount,
        decimal ChangeAmount,
        string PaymentMethod,
        string? Notes,
        List<CreateSaleItemCommand> Items);

    public record OpenShiftCommand(Guid CashierId, decimal StartingCash);
    public record CloseShiftCommand(Guid ShiftId, decimal ActualCashInDrawer, string? Notes);

    public record DashboardDataDto(
        decimal TotalSales,
        decimal NetProfit,
        decimal TotalPurchases,
        decimal TotalExpenses,
        int LowStockCount,
        List<TopProductDto> TopSellingProducts);

    public record TopProductDto(Guid ProductId, string Name, decimal TotalQuantitySold, decimal TotalRevenue);
}
