using Dapper;
using POS.Shared.Application.Database;
using POS.Shared.Application.Messaging;
using POS.Shared.Domain;

namespace Inventory.Application.Catalog.Products.Queries.GetProducts
{
    internal sealed class GetProductsQueryHandler : IQueryHandler<GetProductsQuery, IReadOnlyList<ProductResponse>>
    {
        private readonly ISqlConnectionFactory _sqlConnectionFactory;

        public GetProductsQueryHandler(ISqlConnectionFactory sqlConnectionFactory)
        {
            _sqlConnectionFactory = sqlConnectionFactory;
        }

        public async Task<Result<IReadOnlyList<ProductResponse>>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
        {
            using var connection = _sqlConnectionFactory.CreateConnection();

            var offset = (request.PageNumber - 1) * request.PageSize;

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
                FROM [Inventory].[Products] p
                LEFT JOIN [Inventory].[Categories] c ON p.CategoryId = c.Id
                LEFT JOIN [Inventory].[Brands] b ON p.BrandId = b.Id
                LEFT JOIN [Inventory].[Units] u ON p.UnitId = u.Id
                WHERE (@SearchTerm IS NULL OR p.Name LIKE '%' + @SearchTerm + '%' OR p.Sku LIKE '%' + @SearchTerm + '%')
                  AND (@CategoryId IS NULL OR p.CategoryId = @CategoryId)
                  AND (@BrandId IS NULL OR p.BrandId = @BrandId)
                  AND (@OnlyActive IS NULL OR p.IsActive = @OnlyActive)
                ORDER BY p.Name ASC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
                """;

            var products = await connection.QueryAsync<ProductResponse>(sql, new
            {
                request.SearchTerm,
                request.CategoryId,
                request.BrandId,
                request.OnlyActive,
                Offset = offset,
                request.PageSize
            });

            return Result<IReadOnlyList<ProductResponse>>.Success(products.ToList());
        }
    }
}
