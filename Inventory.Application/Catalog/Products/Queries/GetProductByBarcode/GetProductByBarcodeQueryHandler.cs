using Dapper;
using Inventory.Domain.Catalog.Products.Errors;
using POS.Shared.Application.Database;
using POS.Shared.Application.IService;
using POS.Shared.Application.Messaging;
using POS.Shared.Domain;

namespace Inventory.Application.Catalog.Products.Queries.GetProductByBarcode
{
    internal sealed class GetProductByBarcodeQueryHandler : IQueryHandler<GetProductByBarcodeQuery, ProductResponse>
    {
        private readonly ISqlConnectionFactory _sqlConnectionFactory;
        private readonly ICacheService _cacheService;

        public GetProductByBarcodeQueryHandler(
            ISqlConnectionFactory sqlConnectionFactory,
            ICacheService cacheService)
        {
            _sqlConnectionFactory = sqlConnectionFactory;
            _cacheService = cacheService;
        }

        public async Task<Result<ProductResponse>> Handle(GetProductByBarcodeQuery request, CancellationToken cancellationToken)
        {
            var cacheKey = $"product_barcode_{request.Barcode}";

            var cachedProduct = await _cacheService.GetAsync<ProductResponse>(cacheKey, cancellationToken);
            if (cachedProduct is not null)
            {
                return Result<ProductResponse>.Success(cachedProduct);
            }

            using var connection = _sqlConnectionFactory.CreateConnection();

            const string sql = """
                SELECT 
                    p.Id, p.Barcode, p.NameAr, p.NameEn, p.Description,
                    p.CategoryId, c.NameAr AS CategoryName,
                    p.UnitId, u.Symbol AS UnitSymbol,
                    p.SupplierId, sup.Name AS SupplierName,
                    p.PurchasePrice, p.SellingPrice, p.WholesalePrice,
                    p.QuantityInStock, p.ReorderLevel, p.MaxStockLevel,
                    p.IsWeighable, p.IsActive, p.TrackExpiry, p.TaxRate, p.ImageUrl,
                    p.CreatedAt, p.UpdatedAt
                FROM [Inventory].[Products] p
                LEFT JOIN [Inventory].[Categories] c ON p.CategoryId = c.Id
                LEFT JOIN [Inventory].[Units] u ON p.UnitId = u.Id
                LEFT JOIN [Purchases].[Suppliers] sup ON p.SupplierId = sup.Id
                WHERE p.Barcode = @Barcode
                """;

            var product = await connection.QuerySingleOrDefaultAsync<ProductResponse>(sql, new { request.Barcode });

            if (product is null)
                return Result<ProductResponse>.Failure(ProductErrors.NotFoundByBarcode(request.Barcode));

            await _cacheService.SetAsync(
                cacheKey,
                product,
                absoluteExpiration: TimeSpan.FromMinutes(5),
                slidingExpiration: TimeSpan.FromMinutes(2),
                ct: cancellationToken);

            return Result<ProductResponse>.Success(product);
        }
    }
}
