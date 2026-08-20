using Dapper;
using POS.Shared.Application.Database;
using POS.Shared.Application.Messaging;
using POS.Shared.Domain;

namespace Sales.Application.Sales.Queries.GetSaleById
{
    internal sealed class GetSaleByIdQueryHandler : IQueryHandler<GetSaleByIdQuery, SaleDetailResponse>
    {
        private readonly ISqlConnectionFactory _sqlConnectionFactory;

        public GetSaleByIdQueryHandler(ISqlConnectionFactory sqlConnectionFactory)
        {
            _sqlConnectionFactory = sqlConnectionFactory;
        }

        public async Task<Result<SaleDetailResponse>> Handle(GetSaleByIdQuery request, CancellationToken cancellationToken)
        {
            using var connection = _sqlConnectionFactory.CreateConnection();

            var sql = """
                SELECT 
                    s.Id, s.InvoiceNumber, s.SaleDate, s.CashierId,
                    u.FullName AS CashierName, s.CustomerId, c.Name AS CustomerName,
                    s.ShiftId, s.SubTotal, s.DiscountAmount, s.TaxAmount, s.TotalAmount,
                    s.PaidAmount, s.ChangeAmount, s.PaymentMethod,
                    CASE s.Status WHEN 1 THEN 'Completed' WHEN 2 THEN 'Cancelled' ELSE 'Completed' END AS Status,
                    s.Notes
                FROM [Sales].[Sales] s
                LEFT JOIN [Identity].[AspNetUsers] u ON s.CashierId = CAST(u.Id AS uniqueidentifier)
                LEFT JOIN [Sales].[Customers] c ON s.CustomerId = c.Id
                WHERE s.Id = @Id
                """;

            var itemsSql = """
                SELECT 
                    si.Id, si.ProductId, p.NameAr AS ProductName, p.Barcode,
                    si.Quantity, si.UnitPrice, si.Discount, si.Tax,
                    (si.Quantity * si.UnitPrice - si.Discount + si.Tax) AS Total
                FROM [Sales].[SaleItems] si
                LEFT JOIN [Inventory].[Products] p ON si.ProductId = p.Id
                WHERE si.SaleId = @SaleId
                """;

            var saleData = await connection.QueryFirstOrDefaultAsync<dynamic>(sql, new { request.Id });

            if (saleData is null)
                return Result<SaleDetailResponse>.Failure(new Error("Sale.NotFound", $"الفاتورة بالرقم '{request.Id}' غير موجودة."));

            var items = await connection.QueryAsync<SaleDetailItemResponse>(itemsSql, new { SaleId = request.Id });

            var response = new SaleDetailResponse(
                (Guid)saleData.Id,
                (string)saleData.InvoiceNumber,
                (DateTime)saleData.SaleDate,
                (Guid)saleData.CashierId,
                (string?)saleData.CashierName,
                saleData.CustomerId == null ? (Guid?)null : (Guid)saleData.CustomerId,
                (string?)saleData.CustomerName,
                (Guid)saleData.ShiftId,
                (decimal)saleData.SubTotal,
                (decimal)saleData.DiscountAmount,
                (decimal)saleData.TaxAmount,
                (decimal)saleData.TotalAmount,
                (decimal)saleData.PaidAmount,
                (decimal)saleData.ChangeAmount,
                (string)saleData.PaymentMethod,
                (string)saleData.Status,
                (string?)saleData.Notes,
                items.ToList());

            return Result<SaleDetailResponse>.Success(response);
        }
    }
}
