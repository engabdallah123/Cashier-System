using Dapper;
using POS.Shared.Application.Database;
using POS.Shared.Application.Messaging;
using POS.Shared.Domain;

namespace Inventory.Application.Pricing.ProductPrices.Queries.GetProductPrices
{
    internal sealed class GetProductPricesQueryHandler : IQueryHandler<GetProductPricesQuery, IReadOnlyList<ProductPriceResponse>>
    {
        private readonly ISqlConnectionFactory _sqlConnectionFactory;

        public GetProductPricesQueryHandler(ISqlConnectionFactory sqlConnectionFactory)
        {
            _sqlConnectionFactory = sqlConnectionFactory;
        }

        public async Task<Result<IReadOnlyList<ProductPriceResponse>>> Handle(GetProductPricesQuery request, CancellationToken cancellationToken)
        {
            using var connection = _sqlConnectionFactory.CreateConnection();

            const string sql = """
                SELECT 
                    pp.Id,
                    pp.ProductId,
                    p.Name AS ProductName,
                    p.Sku AS ProductSku,
                    pp.PriceListId,
                    pl.Name AS PriceListName,
                    pp.Price,
                    pp.Currency,
                    pp.CreatedAt,
                    pp.UpdatedAt
                FROM [Inventory].[ProductPrices] pp
                INNER JOIN [Inventory].[Products] p ON pp.ProductId = p.Id
                INNER JOIN [Inventory].[PriceLists] pl ON pp.PriceListId = pl.Id
                WHERE (@ProductId IS NULL OR pp.ProductId = @ProductId)
                  AND (@PriceListId IS NULL OR pp.PriceListId = @PriceListId)
                ORDER BY p.Name ASC
                """;

            var prices = await connection.QueryAsync<ProductPriceResponse>(sql, new
            {
                request.ProductId,
                request.PriceListId
            });

            return Result<IReadOnlyList<ProductPriceResponse>>.Success(prices.ToList());
        }
    }
}
