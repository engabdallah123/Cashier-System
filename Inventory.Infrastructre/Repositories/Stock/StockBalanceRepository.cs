using Inventory.Domain.Stock.StockBalances;
using Inventory.Infrastructre.Database;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Infrastructre.Repositories.Stock
{
    public class StockBalanceRepository : IStockBalanceRepository
    {
        private readonly InventoryDbContext _context;

        public StockBalanceRepository(InventoryDbContext context)
        {
            _context = context;
        }

        public async Task<StockBalance?> GetByProductAndWarehouseAsync(
            Guid productId, Guid warehouseId, CancellationToken cancellationToken = default)
        {
            return await _context.StockBalances
                .FirstOrDefaultAsync(sb => sb.ProductId == productId && sb.WarehouseId == warehouseId, cancellationToken);
        }

        public async Task<IReadOnlyList<StockBalance>> GetByWarehouseAsync(
            Guid warehouseId, CancellationToken cancellationToken = default)
        {
            return await _context.StockBalances
                .Where(sb => sb.WarehouseId == warehouseId)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<StockBalance>> GetByProductAsync(
            Guid productId, CancellationToken cancellationToken = default)
        {
            return await _context.StockBalances
                .Where(sb => sb.ProductId == productId)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<StockBalance>> GetLowStockAsync(
            int threshold, CancellationToken cancellationToken = default)
        {
            return await _context.StockBalances
                .Where(sb => sb.QuantityOnHand <= threshold && sb.QuantityOnHand > 0)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> ExistsAsync(
            Guid productId, Guid warehouseId, CancellationToken cancellationToken = default)
        {
            return await _context.StockBalances
                .AnyAsync(sb => sb.ProductId == productId && sb.WarehouseId == warehouseId, cancellationToken);
        }

        public async Task AddAsync(StockBalance stockBalance, CancellationToken cancellationToken = default)
        {
            await _context.StockBalances.AddAsync(stockBalance, cancellationToken);
        }
    }
}
