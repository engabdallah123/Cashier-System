using Dapper;
using POS.Shared.Application.Database;
using POS.Shared.Application.Messaging;
using POS.Shared.Domain;

namespace Inventory.Application.Stock.StockBalances.Queries.GetStockBalance
{
    internal sealed class GetStockBalanceQueryHandler : IQueryHandler<GetStockBalanceQuery, IReadOnlyList<StockBalanceResponse>>
    {
        private readonly ISqlConnectionFactory _sqlConnectionFactory;

        public GetStockBalanceQueryHandler(ISqlConnectionFactory sqlConnectionFactory)
        {
            _sqlConnectionFactory = sqlConnectionFactory;
        }

        public async Task<Result<IReadOnlyList<StockBalanceResponse>>> Handle(GetStockBalanceQuery request, CancellationToken cancellationToken)
        {
            using var connection = _sqlConnectionFactory.CreateConnection();

            const string sql = """
                SELECT 
                    sb.Id,
                    sb.ProductId,
                    p.Name AS ProductName,
                    p.Sku AS ProductSku,
                    sb.WarehouseId,
                    w.Name AS WarehouseName,
                    sb.QuantityOnHand,
                    sb.LastUpdated
                FROM [Inventory].[StockBalances] sb
                INNER JOIN [Inventory].[Products] p ON sb.ProductId = p.Id
                INNER JOIN [Inventory].[Warehouses] w ON sb.WarehouseId = w.Id
                WHERE (@ProductId IS NULL OR sb.ProductId = @ProductId)
                  AND (@WarehouseId IS NULL OR sb.WarehouseId = @WarehouseId)
                ORDER BY p.Name ASC
                """;

            var balances = await connection.QueryAsync<StockBalanceResponse>(sql, new
            {
                request.ProductId,
                request.WarehouseId
            });

            return Result<IReadOnlyList<StockBalanceResponse>>.Success(balances.ToList());
        }
    }
}
