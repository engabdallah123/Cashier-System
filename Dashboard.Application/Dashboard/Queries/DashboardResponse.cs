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

    public sealed record PeriodMetricsResponse(
        decimal TotalSales,
        decimal NetProfit,
        int TotalInvoices,
        decimal TotalPurchases,
        decimal TotalExpenses);

    public sealed record PaymentMethodSummaryResponse(
        string PaymentMethod,
        decimal TotalAmount,
        int InvoiceCount,
        decimal Percentage);

    public sealed record LowStockProductResponse(
        Guid ProductId,
        string ProductName,
        string Barcode,
        decimal QuantityInStock,
        decimal ReorderLevel);

    public sealed record DashboardResponse(
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
        PeriodMetricsResponse TodayMetrics,
        PeriodMetricsResponse MonthMetrics,
        PeriodMetricsResponse YearMetrics,
        IReadOnlyList<TopProductResponse> TopSellingProducts,
        IReadOnlyList<CashierPerformanceResponse> CashierPerformances,
        IReadOnlyList<PaymentMethodSummaryResponse> PaymentMethodsSummary,
        IReadOnlyList<LowStockProductResponse> LowStockProductsList);
}
