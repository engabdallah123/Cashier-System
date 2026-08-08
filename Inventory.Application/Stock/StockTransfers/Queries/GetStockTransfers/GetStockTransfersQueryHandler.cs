using Dapper;
using POS.Shared.Application.Database;
using POS.Shared.Application.Messaging;
using POS.Shared.Domain;

namespace Inventory.Application.Stock.StockTransfers.Queries.GetStockTransfers
{
    internal sealed class GetStockTransfersQueryHandler : IQueryHandler<GetStockTransfersQuery, IReadOnlyList<StockTransferResponse>>
    {
        private readonly ISqlConnectionFactory _sqlConnectionFactory;

        public GetStockTransfersQueryHandler(ISqlConnectionFactory sqlConnectionFactory)
        {
            _sqlConnectionFactory = sqlConnectionFactory;
        }

        public async Task<Result<IReadOnlyList<StockTransferResponse>>> Handle(GetStockTransfersQuery request, CancellationToken cancellationToken)
        {
            using var connection = _sqlConnectionFactory.CreateConnection();

            const string sql = """
                SELECT 
                    st.Id,
                    st.TransferNumber,
                    st.SourceWarehouseId,
                    sw.Name AS SourceWarehouseName,
                    st.DestinationWarehouseId,
                    dw.Name AS DestinationWarehouseName,
                    st.Status,
                    CASE st.Status
                        WHEN 1 THEN 'Draft'
                        WHEN 2 THEN 'Pending'
                        WHEN 3 THEN 'Executed'
                        WHEN 4 THEN 'Cancelled'
                        ELSE 'Unknown'
                    END AS StatusName,
                    st.Notes,
                    st.CreatedBy,
                    st.CreatedAt,
                    st.ExecutedAt
                FROM [Inventory].[StockTransfers] st
                INNER JOIN [Inventory].[Warehouses] sw ON st.SourceWarehouseId = sw.Id
                INNER JOIN [Inventory].[Warehouses] dw ON st.DestinationWarehouseId = dw.Id
                WHERE (@SourceWarehouseId IS NULL OR st.SourceWarehouseId = @SourceWarehouseId)
                  AND (@DestinationWarehouseId IS NULL OR st.DestinationWarehouseId = @DestinationWarehouseId)
                  AND (@Status IS NULL OR st.Status = @Status)
                ORDER BY st.CreatedAt DESC
                """;

            var transfers = await connection.QueryAsync<StockTransferResponse>(sql, new
            {
                request.SourceWarehouseId,
                request.DestinationWarehouseId,
                request.Status
            });

            return Result<IReadOnlyList<StockTransferResponse>>.Success(transfers.ToList());
        }
    }
}
