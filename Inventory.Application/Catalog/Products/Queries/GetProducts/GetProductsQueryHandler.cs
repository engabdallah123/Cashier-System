using Dapper;
using POS.Shared.Application.Database;
using POS.Shared.Application.IService;
using POS.Shared.Application.Messaging;
using POS.Shared.Domain;

namespace Inventory.Application.Catalog.Products.Queries.GetProducts
{
    internal sealed class GetProductsQueryHandler : IQueryHandler<GetProductsQuery, IReadOnlyList<ProductResponse>>
    {
        private readonly ISqlConnectionFactory _sqlConnectionFactory;
        private readonly ICacheService _cacheService;

        public GetProductsQueryHandler(
            ISqlConnectionFactory sqlConnectionFactory,
            ICacheService cacheService)
        {
            _sqlConnectionFactory = sqlConnectionFactory;
            _cacheService = cacheService;
        }

        public async Task<Result<IReadOnlyList<ProductResponse>>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
        {
            var cacheKey = $"products_list_cat_{request.CategoryId}_term_{request.SearchTerm}_active_{request.IsActive}_page_{request.Page}_size_{request.PageSize}";

            var cachedProducts = await _cacheService.GetAsync<IReadOnlyList<ProductResponse>>(cacheKey, cancellationToken);
            if (cachedProducts is not null)
            {
                return Result<IReadOnlyList<ProductResponse>>.Success(cachedProducts);
            }

            using var connection = _sqlConnectionFactory.CreateConnection();

            var sql = """
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
                WHERE 1 = 1
                """;

            if (request.CategoryId.HasValue)
                sql += " AND p.CategoryId = @CategoryId";

            if (request.IsActive.HasValue)
                sql += " AND p.IsActive = @IsActive";

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                sql += " AND (p.NameAr LIKE @SearchStr OR p.NameEn LIKE @SearchStr OR p.Barcode LIKE @SearchStr)";

            sql += " ORDER BY p.CreatedAt DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            var offset = (request.Page - 1) * request.PageSize;

            var products = await connection.QueryAsync<ProductResponse>(sql, new
            {
                request.CategoryId,
                request.IsActive,
                SearchStr = $"%{request.SearchTerm}%",
                Offset = offset,
                request.PageSize
            });

            var resultList = (IReadOnlyList<ProductResponse>)products.ToList();

            await _cacheService.SetAsync(
                cacheKey,
                resultList,
                absoluteExpiration: TimeSpan.FromMinutes(5),
                slidingExpiration: TimeSpan.FromMinutes(2),
                ct: cancellationToken);

            return Result<IReadOnlyList<ProductResponse>>.Success(resultList);
        }
    }
}
