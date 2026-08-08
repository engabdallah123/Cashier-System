using Dapper;
using POS.Shared.Application.Database;
using POS.Shared.Application.Messaging;
using POS.Shared.Domain;

namespace Inventory.Application.Batches.ProductBatches.Queries.GetExpiringBatches
{
    internal sealed class GetExpiringBatchesQueryHandler : IQueryHandler<GetExpiringBatchesQuery, IReadOnlyList<ProductBatchResponse>>
    {
        private readonly ISqlConnectionFactory _sqlConnectionFactory;

        public GetExpiringBatchesQueryHandler(ISqlConnectionFactory sqlConnectionFactory)
        {
            _sqlConnectionFactory = sqlConnectionFactory;
        }

        public async Task<Result<IReadOnlyList<ProductBatchResponse>>> Handle(GetExpiringBatchesQuery request, CancellationToken cancellationToken)
        {
            using var connection = _sqlConnectionFactory.CreateConnection();

            var thresholdDate = DateTime.UtcNow.Date.AddDays(request.DaysThreshold);
            var today = DateTime.UtcNow.Date;

            const string sql = """
                SELECT 
                    pb.Id,
                    pb.ProductId,
                    p.Name AS ProductName,
                    p.Sku AS ProductSku,
                    pb.WarehouseId,
                    w.Name AS WarehouseName,
                    pb.BatchNumber,
                    pb.ExpiryDate,
                    pb.Quantity,
                    CASE WHEN pb.ExpiryDate IS NOT NULL AND CAST(pb.ExpiryDate AS DATE) <= @Today THEN 1 ELSE 0 END AS IsExpired,
                    CASE WHEN pb.ExpiryDate IS NOT NULL AND CAST(pb.ExpiryDate AS DATE) > @Today AND CAST(pb.ExpiryDate AS DATE) <= @ThresholdDate THEN 1 ELSE 0 END AS IsExpiringSoon,
                    pb.CreatedAt
                FROM [Inventory].[ProductBatches] pb
                INNER JOIN [Inventory].[Products] p ON pb.ProductId = p.Id
                INNER JOIN [Inventory].[Warehouses] w ON pb.WarehouseId = w.Id
                WHERE pb.ExpiryDate IS NOT NULL 
                  AND CAST(pb.ExpiryDate AS DATE) <= @ThresholdDate
                  AND (@WarehouseId IS NULL OR pb.WarehouseId = @WarehouseId)
                  AND pb.Quantity > 0
                ORDER BY pb.ExpiryDate ASC
                """;

            var batches = await connection.QueryAsync<ProductBatchResponse>(sql, new
            {
                Today = today,
                ThresholdDate = thresholdDate,
                request.WarehouseId
            });

            return Result<IReadOnlyList<ProductBatchResponse>>.Success(batches.ToList());
        }
    }
}
