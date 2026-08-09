using Dapper;
using POS.Shared.Application.Database;
using POS.Shared.Application.Messaging;
using POS.Shared.Domain;

namespace Expenses.Application.Expenses.Queries.GetExpenses
{
    internal sealed class GetExpensesQueryHandler : IQueryHandler<GetExpensesQuery, IReadOnlyList<ExpenseResponse>>
    {
        private readonly ISqlConnectionFactory _sqlConnectionFactory;

        public GetExpensesQueryHandler(ISqlConnectionFactory sqlConnectionFactory)
        {
            _sqlConnectionFactory = sqlConnectionFactory;
        }

        public async Task<Result<IReadOnlyList<ExpenseResponse>>> Handle(GetExpensesQuery request, CancellationToken cancellationToken)
        {
            using var connection = _sqlConnectionFactory.CreateConnection();

            var sql = """
                SELECT 
                    e.Id, e.Title, e.Description, e.Amount, e.ExpenseDate,
                    e.CreatedByUserId, u.FullName AS CreatedByName,
                    e.Notes, e.CreatedAt
                FROM [Expenses].[Expenses] e
                LEFT JOIN [Identity].[AspNetUsers] u ON e.CreatedByUserId = CAST(u.Id AS uniqueidentifier)
                WHERE 1 = 1
                """;

            if (request.FromDate.HasValue)
                sql += " AND e.ExpenseDate >= @FromDate";

            if (request.ToDate.HasValue)
                sql += " AND e.ExpenseDate <= @ToDate";

            sql += " ORDER BY e.ExpenseDate DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            var offset = (request.Page - 1) * request.PageSize;

            var expenses = await connection.QueryAsync<ExpenseResponse>(sql, new
            {
                request.FromDate,
                request.ToDate,
                Offset = offset,
                request.PageSize
            });

            return Result<IReadOnlyList<ExpenseResponse>>.Success(expenses.ToList());
        }
    }
}
