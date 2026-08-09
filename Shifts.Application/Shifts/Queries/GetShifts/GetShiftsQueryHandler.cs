using Dapper;
using POS.Shared.Application.Database;
using POS.Shared.Application.Messaging;
using POS.Shared.Domain;

namespace Shifts.Application.Shifts.Queries.GetShifts
{
    internal sealed class GetShiftsQueryHandler : IQueryHandler<GetShiftsQuery, IReadOnlyList<ShiftResponse>>
    {
        private readonly ISqlConnectionFactory _sqlConnectionFactory;

        public GetShiftsQueryHandler(ISqlConnectionFactory sqlConnectionFactory)
        {
            _sqlConnectionFactory = sqlConnectionFactory;
        }

        public async Task<Result<IReadOnlyList<ShiftResponse>>> Handle(GetShiftsQuery request, CancellationToken cancellationToken)
        {
            using var connection = _sqlConnectionFactory.CreateConnection();

            var sql = """
                SELECT 
                    s.Id,
                    s.CashierId,
                    u.FullName AS CashierName,
                    s.OpenedAt,
                    s.ClosedAt,
                    s.OpeningCash,
                    s.ClosingCash,
                    s.SystemCash,
                    s.CashDifference,
                    CASE s.Status 
                        WHEN 1 THEN 'Open'
                        WHEN 2 THEN 'Closed'
                        WHEN 3 THEN 'Cancelled'
                        ELSE 'Unknown'
                    END AS Status,
                    s.TotalSales,
                    s.TotalCash,
                    s.TotalCard,
                    s.TotalWallet,
                    s.TotalCredit,
                    s.TotalDiscount,
                    s.TotalTax,
                    s.TotalInvoices,
                    s.TotalReturns,
                    s.Notes,
                    s.ClosingNotes
                FROM [Shifts].[Shifts] s
                LEFT JOIN [Identity].[AspNetUsers] u ON s.CashierId = CAST(u.Id AS uniqueidentifier)
                WHERE 1 = 1
                """;

            if (request.CashierId.HasValue)
                sql += " AND s.CashierId = @CashierId";

            if (request.FromDate.HasValue)
                sql += " AND s.OpenedAt >= @FromDate";

            if (request.ToDate.HasValue)
                sql += " AND s.OpenedAt <= @ToDate";

            sql += " ORDER BY s.OpenedAt DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            var offset = (request.Page - 1) * request.PageSize;

            var shifts = await connection.QueryAsync<ShiftResponse>(sql, new
            {
                request.CashierId,
                request.FromDate,
                request.ToDate,
                Offset = offset,
                request.PageSize
            });

            return Result<IReadOnlyList<ShiftResponse>>.Success(shifts.ToList());
        }
    }
}
