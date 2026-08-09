namespace Dashboard.Application.Dashboard.Queries
{
    public sealed record TopProductResponse(
        Guid ProductId,
        string ProductName,
        string Barcode,
        decimal TotalQuantitySold,
        decimal TotalRevenue);

    public sealed record CashierPerformanceResponse(
        Guid CashierId,
        string CashierName,
        int TotalShifts,
        int TotalInvoices,
        decimal TotalSalesAmount,
        decimal TotalCashDifference);

    public sealed record DashboardResponse(
        decimal TotalSales,
        int TotalInvoices,
        decimal TotalPurchases,
        decimal TotalExpenses,
        decimal NetProfit,
        decimal TotalSalesReturns,
        decimal TotalPurchaseReturns,
        int LowStockProductsCount,
        IReadOnlyList<TopProductResponse> TopSellingProducts,
        IReadOnlyList<CashierPerformanceResponse> CashierPerformances);
}
