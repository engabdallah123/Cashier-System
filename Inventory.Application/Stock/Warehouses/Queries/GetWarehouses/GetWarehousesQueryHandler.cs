using Dapper;
using POS.Shared.Application.Database;
using POS.Shared.Application.Messaging;
using POS.Shared.Domain;

namespace Inventory.Application.Stock.Warehouses.Queries.GetWarehouses
{
    internal sealed class GetWarehousesQueryHandler : IQueryHandler<GetWarehousesQuery, IReadOnlyList<WarehouseResponse>>
    {
        private readonly ISqlConnectionFactory _sqlConnectionFactory;

        public GetWarehousesQueryHandler(ISqlConnectionFactory sqlConnectionFactory)
        {
            _sqlConnectionFactory = sqlConnectionFactory;
        }

        public async Task<Result<IReadOnlyList<WarehouseResponse>>> Handle(GetWarehousesQuery request, CancellationToken cancellationToken)
        {
            using var connection = _sqlConnectionFactory.CreateConnection();

            const string sql = """
                SELECT Id, Name, Code, Address, IsActive
                FROM [Inventory].[Warehouses]
                WHERE (@OnlyActive IS NULL OR IsActive = @OnlyActive)
                ORDER BY Name ASC
                """;

            var warehouses = await connection.QueryAsync<WarehouseResponse>(sql, new { request.OnlyActive });
            return Result<IReadOnlyList<WarehouseResponse>>.Success(warehouses.ToList());
        }
    }
}
