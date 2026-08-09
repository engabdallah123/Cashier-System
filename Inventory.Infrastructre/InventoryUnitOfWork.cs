using Inventory.Domain;
using Inventory.Domain.Catalog.Categories;
using Inventory.Domain.Catalog.Products.Interface;
using Inventory.Domain.Catalog.Units;
using Inventory.Domain.Stock.StockMovements;
using Inventory.Infrastructre.Database;
using Inventory.Infrastructre.Repositories.Catalog;
using POS.Shared.Domain;
using POS.Shared.Infrastructure.Database;

namespace Inventory.Infrastructre
{
    public class InventoryUnitOfWork : IInventoryUnitOfWork
    {
        private readonly InventoryDbContext _dbContext;

        public IProductRepository ProductRepository { get; private set; }
        public IBaseRepository<Category> CategoryRepository { get; private set; }
        public IBaseRepository<Unit> UnitRepository { get; private set; }
        public IBaseRepository<StockMovement> StockMovementRepository { get; private set; }

        public InventoryUnitOfWork(InventoryDbContext dbContext)
        {
            _dbContext = dbContext;
            ProductRepository = new ProductRepository(_dbContext);
            CategoryRepository = new BaseRepository<Category>(_dbContext);
            UnitRepository = new BaseRepository<Unit>(_dbContext);
            StockMovementRepository = new BaseRepository<StockMovement>(_dbContext);
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
