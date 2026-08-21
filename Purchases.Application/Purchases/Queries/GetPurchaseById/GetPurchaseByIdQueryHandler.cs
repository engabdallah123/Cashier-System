using Dapper;
using POS.Shared.Application.Database;
using POS.Shared.Application.Messaging;
using POS.Shared.Domain;

namespace Purchases.Application.Purchases.Queries.GetPurchaseById
{
    internal sealed class GetPurchaseByIdQueryHandler : IQueryHandler<GetPurchaseByIdQuery, PurchaseDetailResponse>
    {
        private readonly ISqlConnectionFactory _sqlConnectionFactory;

        public GetPurchaseByIdQueryHandler(ISqlConnectionFactory sqlConnectionFactory)
        {
            _sqlConnectionFactory = sqlConnectionFactory;
        }

        public async Task<Result<PurchaseDetailResponse>> Handle(GetPurchaseByIdQuery request, CancellationToken cancellationToken)
        {
            using var connection = _sqlConnectionFactory.CreateConnection();

            var sql = """
                SELECT 
                    p.Id, p.InvoiceNumber, p.InternalNumber, p.PurchaseDate,
                    p.SupplierId, s.Name AS SupplierName,
                    p.SubTotal, p.DiscountAmount, p.TaxAmount, p.TotalAmount,
                    p.PaidAmount, p.RemainingAmount,
                    CASE p.Status WHEN 1 THEN 'Draft' WHEN 2 THEN 'Received' WHEN 3 THEN 'Cancelled' ELSE 'Draft' END AS Status,
                    p.Notes
                FROM [Purchases].[Purchases] p
                LEFT JOIN [Purchases].[Suppliers] s ON p.SupplierId = s.Id
                WHERE p.Id = @Id
                """;

            var itemsSql = """
                SELECT 
                    pi2.Id, pi2.ProductId, pr.NameAr AS ProductName, pr.Barcode,
                    pi2.Quantity, pi2.UnitCost, pi2.Discount, pi2.Tax,
                    (pi2.Quantity * pi2.UnitCost - pi2.Discount + pi2.Tax) AS Total,
                    pi2.ExpiryDate, pi2.BatchNumber
                FROM [Purchases].[PurchaseItems] pi2
                LEFT JOIN [Inventory].[Products] pr ON pi2.ProductId = pr.Id
                WHERE pi2.PurchaseId = @PurchaseId
                """;

            var purchaseData = await connection.QueryFirstOrDefaultAsync<dynamic>(sql, new { request.Id });

            if (purchaseData is null)
                return Result<PurchaseDetailResponse>.Failure(new Error("Purchase.NotFound", $"فاتورة الشراء بالرقم '{request.Id}' غير موجودة."));

            var items = await connection.QueryAsync<PurchaseDetailItemResponse>(itemsSql, new { PurchaseId = request.Id });

            var response = new PurchaseDetailResponse(
                (Guid)purchaseData.Id,
                (string)purchaseData.InvoiceNumber,
                (string?)purchaseData.InternalNumber,
                (DateTime)purchaseData.PurchaseDate,
                (Guid)purchaseData.SupplierId,
                (string?)purchaseData.SupplierName,
                (decimal)purchaseData.SubTotal,
                (decimal)purchaseData.DiscountAmount,
                (decimal)purchaseData.TaxAmount,
                (decimal)purchaseData.TotalAmount,
                (decimal)purchaseData.PaidAmount,
                (decimal)purchaseData.RemainingAmount,
                (string)purchaseData.Status,
                (string?)purchaseData.Notes,
                items.ToList());

            return Result<PurchaseDetailResponse>.Success(response);
        }
    }
}
