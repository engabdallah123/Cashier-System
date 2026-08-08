using Dapper;
using Inventory.Domain.Catalog.Products.Errors;
using POS.Shared.Application.Database;
using POS.Shared.Application.Messaging;
using POS.Shared.Domain;

namespace Inventory.Application.Catalog.Products.Queries.GetProductByBarcode
{
    internal sealed class GetProductByBarcodeQueryHandler : IQueryHandler<GetProductByBarcodeQuery, ProductResponse>
    {
        private readonly ISqlConnectionFactory _sqlConnectionFactory;

        public GetProductByBarcodeQueryHandler(ISqlConnectionFactory sqlConnectionFactory)
        {
            _sqlConnectionFactory = sqlConnectionFactory;
        }

        public async Task<Result<ProductResponse>> Handle(GetProductByBarcodeQuery request, CancellationToken cancellationToken)
        {
            using var connection = _sqlConnectionFactory.CreateConnection();

            const string sql = """
                SELECT 
                    p.Id,
                    p.Name,
                    p.Sku,
                    p.Price,
                    p.Currency,
                    p.QuantityOnHand,
                    p.LowStockThreshold,
                    p.IsActive,
                    p.CategoryId,
                    c.Name AS CategoryName,
                    p.BrandId,
                    b.Name AS BrandName,
                    p.UnitId,
                    u.Name AS UnitName,
                    p.CreatedAt,
                    p.UpdatedAt
                FROM [Inventory].[ProductBarcodes] pb
                INNER JOIN [Inventory].[Products] p ON pb.ProductId = p.Id
                LEFT JOIN [Inventory].[Categories] c ON p.CategoryId = c.Id
                LEFT JOIN [Inventory].[Brands] b ON p.BrandId = b.Id
                LEFT JOIN [Inventory].[Units] u ON p.UnitId = u.Id
                WHERE pb.Barcode = @Barcode
                """;

            var product = await connection.QuerySingleOrDefaultAsync<ProductResponse>(sql, new { request.Barcode });

            if (product is null)
                return Result<ProductResponse>.Failure(ProductErrors.NotFoundByBarcode(request.Barcode));

            return Result<ProductResponse>.Success(product);
        }
    }
}
