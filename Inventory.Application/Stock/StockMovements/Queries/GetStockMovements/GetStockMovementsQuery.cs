using POS.Shared.Application.Messaging;

namespace Inventory.Application.Stock.StockMovements.Queries.GetStockMovements
{
    public sealed record GetStockMovementsQuery(
        Guid? ProductId = null,
        Guid? WarehouseId = null,
        int PageNumber = 1,
        int PageSize = 50) : IQuery<IReadOnlyList<StockMovementResponse>>;
}
