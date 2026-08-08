using Dapper;
using Inventory.Domain.Catalog.Products.Errors;
using POS.Shared.Application.Database;
using POS.Shared.Application.Messaging;
using POS.Shared.Domain;

namespace Inventory.Application.Catalog.Products.Queries.GetProductById
{
    internal sealed class GetProductByIdQueryHandler : IQueryHandler<GetProductByIdQuery, ProductResponse>
    {
        private readonly ISqlConnectionFactory _sqlConnectionFactory;

        public GetProductByIdQueryHandler(ISqlConnectionFactory sqlConnectionFactory)
        {
            _sqlConnectionFactory = sqlConnectionFactory;
        }

        public async Task<Result<ProductResponse>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
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
                FROM [Inventory].[Products] p
                LEFT JOIN [Inventory].[Categories] c ON p.CategoryId = c.Id
                LEFT JOIN [Inventory].[Brands] b ON p.BrandId = b.Id
                LEFT JOIN [Inventory].[Units] u ON p.UnitId = u.Id
                WHERE p.Id = @Id
                """;

            var product = await connection.QuerySingleOrDefaultAsync<ProductResponse>(sql, new { request.Id });

            if (product is null)
                return Result<ProductResponse>.Failure(ProductErrors.NotFound(request.Id));

            return Result<ProductResponse>.Success(product);
        }
    }
}
