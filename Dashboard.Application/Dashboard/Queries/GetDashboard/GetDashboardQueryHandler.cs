using Dapper;
using POS.Shared.Application.Database;
using POS.Shared.Application.Messaging;
using POS.Shared.Domain;
using System.Data;

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

            DateTime fromDate = request.FromDate.HasValue
                ? (request.FromDate.Value.Kind == DateTimeKind.Utc ? request.FromDate.Value : DateTime.SpecifyKind(request.FromDate.Value, DateTimeKind.Local).ToUniversalTime())
                : new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            DateTime toDate = request.ToDate.HasValue
                ? (request.ToDate.Value.Kind == DateTimeKind.Utc ? request.ToDate.Value : DateTime.SpecifyKind(request.ToDate.Value, DateTimeKind.Local).ToUniversalTime())
                : DateTime.UtcNow.AddDays(1);

            const string salesSql = """
                SELECT 
                    ISNULL(SUM(TotalAmount), 0) AS TotalSales,
                    COUNT(1) AS TotalInvoices
                FROM [Sales].[Sales]
                WHERE Status IN (1, 4) AND SaleDate >= @FromDate AND SaleDate <= @ToDate
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

            const string lowStockListSql = """
                SELECT TOP 10 
                    Id AS ProductId,
                    NameAr AS ProductName,
                    Barcode,
                    QuantityInStock,
                    ReorderLevel
                FROM [Inventory].[Products]
                WHERE IsActive = 1 AND QuantityInStock <= ReorderLevel
                ORDER BY QuantityInStock ASC
                """;
            var lowStockList = (await connection.QueryAsync<LowStockProductResponse>(lowStockListSql)).ToList();

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
                WHERE s.Status IN (1, 4) AND s.SaleDate >= @FromDate AND s.SaleDate <= @ToDate
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

            const string paymentMethodsSql = """
                SELECT 
                    PaymentMethod,
                    ISNULL(SUM(TotalAmount), 0) AS TotalAmount,
                    COUNT(1) AS InvoiceCount
                FROM [Sales].[Sales]
                WHERE Status IN (1, 4) AND SaleDate >= @FromDate AND SaleDate <= @ToDate
                GROUP BY PaymentMethod
                """;
            var rawPaymentMethods = (await connection.QueryAsync(paymentMethodsSql, new { FromDate = fromDate, ToDate = toDate })).ToList();
            var paymentMethodsSummary = rawPaymentMethods.Select(p => {
                string method = Convert.ToString(p.PaymentMethod) ?? "Cash";
                decimal amount = Convert.ToDecimal(p.TotalAmount);
                int count = Convert.ToInt32(p.InvoiceCount);
                decimal pct = totalSales > 0 ? (amount / totalSales) * 100 : 0;
                return new PaymentMethodSummaryResponse(method, amount, count, Math.Round(pct, 1));
            }).ToList();

            var nowLocal = DateTime.Now;
            var todayStart = DateTime.SpecifyKind(nowLocal.Date, DateTimeKind.Local).ToUniversalTime();
            var todayEnd = DateTime.SpecifyKind(nowLocal.Date.AddDays(1).AddTicks(-1), DateTimeKind.Local).ToUniversalTime();

            var monthStart = DateTime.SpecifyKind(new DateTime(nowLocal.Year, nowLocal.Month, 1), DateTimeKind.Local).ToUniversalTime();
            var monthEnd = DateTime.SpecifyKind(new DateTime(nowLocal.Year, nowLocal.Month, 1).AddMonths(1).AddTicks(-1), DateTimeKind.Local).ToUniversalTime();

            var yearStart = DateTime.SpecifyKind(new DateTime(nowLocal.Year, 1, 1), DateTimeKind.Local).ToUniversalTime();
            var yearEnd = DateTime.SpecifyKind(new DateTime(nowLocal.Year, 1, 1).AddYears(1).AddTicks(-1), DateTimeKind.Local).ToUniversalTime();

            var todayMetrics = await GetPeriodMetricsAsync(connection, todayStart, todayEnd);
            var monthMetrics = await GetPeriodMetricsAsync(connection, monthStart, monthEnd);
            var yearMetrics = await GetPeriodMetricsAsync(connection, yearStart, yearEnd);

            decimal netProfit = (totalSales - totalSalesReturns) - (totalPurchases - totalPurchaseReturns) - totalExpenses;
            decimal avgInvoiceValue = totalInvoices > 0 ? totalSales / totalInvoices : 0;
            decimal profitMarginPct = totalSales > 0 ? (netProfit / totalSales) * 100 : 0;

            var dashboard = new DashboardResponse(
                totalSales,
                totalInvoices,
                totalPurchases,
                totalExpenses,
                netProfit,
                totalSalesReturns,
                totalPurchaseReturns,
                avgInvoiceValue,
                profitMarginPct,
                lowStockCount,
                todayMetrics,
                monthMetrics,
                yearMetrics,
                topProducts,
                cashierPerformances,
                paymentMethodsSummary,
                lowStockList);

            return Result<DashboardResponse>.Success(dashboard);
        }

        private static async Task<PeriodMetricsResponse> GetPeriodMetricsAsync(IDbConnection connection, DateTime start, DateTime end)
        {
            const string periodSql = """
                SELECT 
                    (SELECT ISNULL(SUM(TotalAmount), 0) FROM [Sales].[Sales] WHERE Status IN (1, 4) AND SaleDate >= @Start AND SaleDate <= @End) AS TotalSales,
                    (SELECT COUNT(1) FROM [Sales].[Sales] WHERE Status IN (1, 4) AND SaleDate >= @Start AND SaleDate <= @End) AS TotalInvoices,
                    (SELECT ISNULL(SUM(TotalAmount), 0) FROM [Purchases].[Purchases] WHERE Status = 2 AND PurchaseDate >= @Start AND PurchaseDate <= @End) AS TotalPurchases,
                    (SELECT ISNULL(SUM(Amount), 0) FROM [Expenses].[Expenses] WHERE ExpenseDate >= @Start AND ExpenseDate <= @End) AS TotalExpenses,
                    (SELECT ISNULL(SUM(TotalAmount), 0) FROM [Returns].[SalesReturns] WHERE Status = 1 AND ReturnDate >= @Start AND ReturnDate <= @End) AS SalesReturns,
                    (SELECT ISNULL(SUM(TotalAmount), 0) FROM [Returns].[PurchaseReturns] WHERE Status = 1 AND ReturnDate >= @Start AND ReturnDate <= @End) AS PurchaseReturns
                """;

            var raw = await connection.QuerySingleAsync(periodSql, new { Start = start, End = end });
            decimal sales = Convert.ToDecimal(raw.TotalSales);
            int invoices = Convert.ToInt32(raw.TotalInvoices);
            decimal purchases = Convert.ToDecimal(raw.TotalPurchases);
            decimal expenses = Convert.ToDecimal(raw.TotalExpenses);
            decimal salesReturns = Convert.ToDecimal(raw.SalesReturns);
            decimal purchaseReturns = Convert.ToDecimal(raw.PurchaseReturns);

            decimal netProfit = (sales - salesReturns) - (purchases - purchaseReturns) - expenses;

            return new PeriodMetricsResponse(sales, netProfit, invoices, purchases, expenses);
        }
    }
}
