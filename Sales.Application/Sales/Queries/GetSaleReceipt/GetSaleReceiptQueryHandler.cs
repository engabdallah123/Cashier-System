using Dapper;
using POS.Shared.Application.Database;
using POS.Shared.Application.Messaging;
using POS.Shared.Domain;
using Sales.Domain.Sales;

namespace Sales.Application.Sales.Queries.GetSaleReceipt
{
    internal sealed class GetSaleReceiptQueryHandler : IQueryHandler<GetSaleReceiptQuery, ReceiptResponse>
    {
        private readonly ISqlConnectionFactory _sqlConnectionFactory;

        public GetSaleReceiptQueryHandler(ISqlConnectionFactory sqlConnectionFactory)
        {
            _sqlConnectionFactory = sqlConnectionFactory;
        }

        public async Task<Result<ReceiptResponse>> Handle(GetSaleReceiptQuery request, CancellationToken cancellationToken)
        {
            using var connection = _sqlConnectionFactory.CreateConnection();

            const string settingsSql = """
                SELECT TOP 1
                    StoreName, Address, Phone, Currency, InvoiceFooterMessage
                FROM [Settings].[StoreSettings]
                """;

            var setting = await connection.QuerySingleOrDefaultAsync(settingsSql);
            var storeName = setting?.StoreName ?? "Supermarket POS";
            var address = setting?.Address;
            var phone = setting?.Phone;
            var currency = setting?.Currency ?? "EGP";
            var invoiceFooterMessage = setting?.InvoiceFooterMessage ?? "شكراً لزيارتكم!";

            const string saleSql = """
                SELECT 
                    s.InvoiceNumber, s.SaleDate,
                    ISNULL(u.FullName, 'Cashier') AS CashierName,
                    c.Name AS CustomerName,
                    s.SubTotal, s.DiscountAmount, s.TaxAmount, s.TotalAmount,
                    s.PaidAmount, s.ChangeAmount, s.PaymentMethod
                FROM [Sales].[Sales] s
                LEFT JOIN [Identity].[AspNetUsers] u ON s.CashierId = CAST(u.Id AS uniqueidentifier)
                LEFT JOIN [Sales].[Customers] c ON s.CustomerId = c.Id
                WHERE s.Id = @SaleId
                """;

            var saleHeader = await connection.QuerySingleOrDefaultAsync(saleSql, new { request.SaleId });
            if (saleHeader is null)
                return Result<ReceiptResponse>.Failure(SaleErrors.NotFound(request.SaleId));

            const string itemsSql = """
                SELECT 
                    i.Id, i.ProductId, p.NameAr AS ProductName, p.Barcode,
                    i.Quantity, i.UnitPrice, i.Discount, i.Tax, i.Total
                FROM [Sales].[SaleItems] i
                LEFT JOIN [Inventory].[Products] p ON i.ProductId = p.Id
                WHERE i.SaleId = @SaleId
                """;

            var items = (await connection.QueryAsync<SaleItemResponse>(itemsSql, new { request.SaleId })).ToList();

            var receipt = new ReceiptResponse(
                storeName,
                address,
                phone,
                saleHeader.InvoiceNumber,
                saleHeader.SaleDate,
                saleHeader.CashierName,
                saleHeader.CustomerName,
                items,
                saleHeader.SubTotal,
                saleHeader.DiscountAmount,
                saleHeader.TaxAmount,
                saleHeader.TotalAmount,
                saleHeader.PaidAmount,
                saleHeader.ChangeAmount,
                saleHeader.PaymentMethod,
                currency,
                invoiceFooterMessage);

            return Result<ReceiptResponse>.Success(receipt);
        }
    }
}
