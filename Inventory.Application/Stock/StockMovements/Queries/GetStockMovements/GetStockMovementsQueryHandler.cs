using Dapper;
using Inventory.Domain.Stock.StockMovements;
using POS.Shared.Application.Database;
using POS.Shared.Application.Messaging;
using POS.Shared.Domain;

namespace Inventory.Application.Stock.StockMovements.Queries.GetStockMovements
{
    internal sealed class GetStockMovementsQueryHandler : IQueryHandler<GetStockMovementsQuery, IReadOnlyList<StockMovementResponse>>
    {
        private readonly ISqlConnectionFactory _sqlConnectionFactory;

        public GetStockMovementsQueryHandler(ISqlConnectionFactory sqlConnectionFactory)
        {
            _sqlConnectionFactory = sqlConnectionFactory;
        }

        public async Task<Result<IReadOnlyList<StockMovementResponse>>> Handle(GetStockMovementsQuery request, CancellationToken cancellationToken)
        {
            using var connection = _sqlConnectionFactory.CreateConnection();

            var offset = (request.PageNumber - 1) * request.PageSize;

            const string sql = """
                SELECT 
                    sm.Id,
                    sm.ProductId,
                    p.Name AS ProductName,
                    p.Sku AS ProductSku,
                    sm.WarehouseId,
                    w.Name AS WarehouseName,
                    sm.Type,
                    CASE sm.Type
                        WHEN 1 THEN 'Purchase'
                        WHEN 2 THEN 'Sale'
                        WHEN 3 THEN 'Return'
                        WHEN 4 THEN 'Adjustment'
                        WHEN 5 THEN 'TransferIn'
                        WHEN 6 THEN 'TransferOut'
                        WHEN 7 THEN 'Damaged'
                        WHEN 8 THEN 'InitialStock'
                        ELSE 'Unknown'
                    END AS TypeName,
                    sm.Quantity,
                    sm.BeforeQuantity,
                    sm.AfterQuantity,
                    sm.ReferenceType,
                    sm.ReferenceId,
                    sm.CreatedAt,
                    sm.CreatedBy
                FROM [Inventory].[StockMovements] sm
                INNER JOIN [Inventory].[Products] p ON sm.ProductId = p.Id
                INNER JOIN [Inventory].[Warehouses] w ON sm.WarehouseId = w.Id
                WHERE (@ProductId IS NULL OR sm.ProductId = @ProductId)
                  AND (@WarehouseId IS NULL OR sm.WarehouseId = @WarehouseId)
                ORDER BY sm.CreatedAt DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
                """;

            var movements = await connection.QueryAsync<StockMovementResponse>(sql, new
            {
                request.ProductId,
                request.WarehouseId,
                Offset = offset,
                request.PageSize
            });

            return Result<IReadOnlyList<StockMovementResponse>>.Success(movements.ToList());
        }
    }
}
