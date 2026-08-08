using Dapper;
using POS.Shared.Application.Database;
using POS.Shared.Application.Messaging;
using POS.Shared.Domain;

namespace Inventory.Application.Catalog.Brands.Queries.GetBrands
{
    internal sealed class GetBrandsQueryHandler : IQueryHandler<GetBrandsQuery, IReadOnlyList<BrandResponse>>
    {
        private readonly ISqlConnectionFactory _sqlConnectionFactory;

        public GetBrandsQueryHandler(ISqlConnectionFactory sqlConnectionFactory)
        {
            _sqlConnectionFactory = sqlConnectionFactory;
        }

        public async Task<Result<IReadOnlyList<BrandResponse>>> Handle(GetBrandsQuery request, CancellationToken cancellationToken)
        {
            using var connection = _sqlConnectionFactory.CreateConnection();

            const string sql = """
                SELECT Id, Name, IsActive, CreatedAt
                FROM [Inventory].[Brands]
                WHERE (@OnlyActive IS NULL OR IsActive = @OnlyActive)
                ORDER BY Name ASC
                """;

            var brands = await connection.QueryAsync<BrandResponse>(sql, new { request.OnlyActive });
            return Result<IReadOnlyList<BrandResponse>>.Success(brands.ToList());
        }
    }
}
