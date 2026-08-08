using Dapper;
using POS.Shared.Application.Database;
using POS.Shared.Application.Messaging;
using POS.Shared.Domain;

namespace Inventory.Application.Pricing.PriceLists.Queries.GetPriceLists
{
    internal sealed class GetPriceListsQueryHandler : IQueryHandler<GetPriceListsQuery, IReadOnlyList<PriceListResponse>>
    {
        private readonly ISqlConnectionFactory _sqlConnectionFactory;

        public GetPriceListsQueryHandler(ISqlConnectionFactory sqlConnectionFactory)
        {
            _sqlConnectionFactory = sqlConnectionFactory;
        }

        public async Task<Result<IReadOnlyList<PriceListResponse>>> Handle(GetPriceListsQuery request, CancellationToken cancellationToken)
        {
            using var connection = _sqlConnectionFactory.CreateConnection();

            const string sql = """
                SELECT Id, Name, Description, IsDefault, IsActive, CreatedAt
                FROM [Inventory].[PriceLists]
                WHERE (@OnlyActive IS NULL OR IsActive = @OnlyActive)
                ORDER BY IsDefault DESC, Name ASC
                """;

            var priceLists = await connection.QueryAsync<PriceListResponse>(sql, new { request.OnlyActive });
            return Result<IReadOnlyList<PriceListResponse>>.Success(priceLists.ToList());
        }
    }
}
