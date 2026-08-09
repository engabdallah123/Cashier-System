using Dapper;
using POS.Shared.Application.Database;
using POS.Shared.Application.Messaging;
using POS.Shared.Domain;

namespace Dashboard.Application.Dashboard.Queries.GetDashboard
{
    internal sealed class GetDashboardQueryHandler : IQueryHandler<GetDashboardQuery, DashboardResponse>
    {
        private readonly ISqlConnectionFactory _sqlConnectionFactory;

        public GetDashboardQueryHandler(ISqlConnectionFactory sqlConnectionFactory)
        {
            _sqlConnectionFactory = sqlConnectionFactory;
        }

        public async Task<Result<DashboardResponse>> Handle(GetDashboardQuery request, CancellationToken cancellationToken)
        {
            using var connection = _sqlConnectionFactory.CreateConnection();

            var fromDate = request.FromDate ?? DateTime.UtcNow.Date;
            var toDate = request.ToDate ?? DateTime.UtcNow;

            const string salesSql = """
                SELECT 
                    ISNULL(SUM(TotalAmount), 0) AS TotalSales,
                    COUNT(1) AS TotalInvoices
                FROM [Sales].[Sales]
                WHERE Status = 1 AND SaleDate >= @FromDate AND SaleDate <= @ToDate
                """;

            var salesMetrics = await connection.QuerySingleAsync(salesSql, new { FromDate = fromDate, ToDate = toDate });
            decimal totalSales = Convert.ToDecimal(salesMetrics.TotalSales);
            int totalInvoices = Convert.ToInt32(salesMetrics.TotalInvoices);

            const string purchasesSql = """
                SELECT ISNULL(SUM(TotalAmount), 0) 
                FROM [Purchases].[Purchases]
                WHERE Status = 2 AND PurchaseDate >= @FromDate AND PurchaseDate <= @ToDate
                """;
            decimal totalPurchases = await connection.QuerySingleAsync<decimal>(purchasesSql, new { FromDate = fromDate, ToDate = toDate });

            const string expensesSql = """
                SELECT ISNULL(SUM(Amount), 0) 
                FROM [Expenses].[Expenses]
                WHERE ExpenseDate >= @FromDate AND ExpenseDate <= @ToDate
                """;
            decimal totalExpenses = await connection.QuerySingleAsync<decimal>(expensesSql, new { FromDate = fromDate, ToDate = toDate });

            const string salesReturnsSql = """
                SELECT ISNULL(SUM(TotalAmount), 0) 
                FROM [Returns].[SalesReturns]
                WHERE Status = 1 AND ReturnDate >= @FromDate AND ReturnDate <= @ToDate
                """;
            decimal totalSalesReturns = await connection.QuerySingleAsync<decimal>(salesReturnsSql, new { FromDate = fromDate, ToDate = toDate });

            const string purchaseReturnsSql = """
                SELECT ISNULL(SUM(TotalAmount), 0) 
                FROM [Returns].[PurchaseReturns]
                WHERE Status = 1 AND ReturnDate >= @FromDate AND ReturnDate <= @ToDate
                """;
            decimal totalPurchaseReturns = await connection.QuerySingleAsync<decimal>(purchaseReturnsSql, new { FromDate = fromDate, ToDate = toDate });

            const string lowStockSql = """
                SELECT COUNT(1) 
                FROM [Inventory].[Products]
                WHERE IsActive = 1 AND QuantityInStock <= ReorderLevel
                """;
            int lowStockCount = await connection.QuerySingleAsync<int>(lowStockSql);

            const string topProductsSql = """
                SELECT TOP 5
                    i.ProductId,
                    p.NameAr AS ProductName,
                    p.Barcode,
                    SUM(i.Quantity) AS TotalQuantitySold,
                    SUM(i.Total) AS TotalRevenue
                FROM [Sales].[SaleItems] i
                JOIN [Sales].[Sales] s ON i.SaleId = s.Id
                LEFT JOIN [Inventory].[Products] p ON i.ProductId = p.Id
                WHERE s.Status = 1 AND s.SaleDate >= @FromDate AND s.SaleDate <= @ToDate
                GROUP BY i.ProductId, p.NameAr, p.Barcode
                ORDER BY TotalRevenue DESC
                """;
            var topProducts = (await connection.QueryAsync<TopProductResponse>(topProductsSql, new { FromDate = fromDate, ToDate = toDate })).ToList();

            const string cashierSql = """
                SELECT 
                    s.CashierId,
                    ISNULL(u.FullName, 'Cashier') AS CashierName,
                    COUNT(DISTINCT s.Id) AS TotalShifts,
                    ISNULL(SUM(s.TotalInvoices), 0) AS TotalInvoices,
                    ISNULL(SUM(s.TotalSales), 0) AS TotalSalesAmount,
                    ISNULL(SUM(s.CashDifference), 0) AS TotalCashDifference
                FROM [Shifts].[Shifts] s
                LEFT JOIN [Identity].[AspNetUsers] u ON s.CashierId = CAST(u.Id AS uniqueidentifier)
                WHERE s.OpenedAt >= @FromDate AND s.OpenedAt <= @ToDate
                GROUP BY s.CashierId, u.FullName
                """;
            var cashierPerformances = (await connection.QueryAsync<CashierPerformanceResponse>(cashierSql, new { FromDate = fromDate, ToDate = toDate })).ToList();

            decimal netProfit = (totalSales - totalSalesReturns) - (totalPurchases - totalPurchaseReturns) - totalExpenses;

            var dashboard = new DashboardResponse(
                totalSales,
                totalInvoices,
                totalPurchases,
                totalExpenses,
                netProfit,
                totalSalesReturns,
                totalPurchaseReturns,
                lowStockCount,
                topProducts,
                cashierPerformances);

            return Result<DashboardResponse>.Success(dashboard);
        }
    }
}
