using Inventory.Domain;
using Inventory.Domain.Batches.ProductBatches;
using Inventory.Domain.Catalog.Brands;
using Inventory.Domain.Catalog.Categories;
using Inventory.Domain.Catalog.ProductBarcodes;
using Inventory.Domain.Catalog.Products.Interface;
using Inventory.Domain.Catalog.Units;
using Inventory.Domain.Pricing.PriceLists;
using Inventory.Domain.Pricing.ProductPrices;
using Inventory.Domain.Stock.StockBalances;
using Inventory.Domain.Stock.StockMovements;
using Inventory.Domain.Stock.StockTransfers;
using Inventory.Domain.Stock.Warehouses;
using Inventory.Infrastructre.Database;
using Inventory.Infrastructre.Repositories.Catalog;
using Inventory.Infrastructre.Repositories.Stock;
using POS.Shared.Domain;
using POS.Shared.Infrastructure.Database;

namespace Inventory.Infrastructre
{
    public class InventoryUnitOfWork : IInventoryUnitOfWork
    {
        private readonly InventoryDbContext _dbContext;

        public IProductRepository ProductRepository { get; private set; }
        public IStockBalanceRepository StockBalanceRepository { get; private set; }

        public IBaseRepository<ProductBarcode> ProductBarcodeRepository { get; private set; }
        public IBaseRepository<Category> CategoryRepository { get; private set; }
        public IBaseRepository<Brand> BrandRepository { get; private set; }
        public IBaseRepository<UnitMeasure> UnitRepository { get; private set; }
        public IBaseRepository<Warehouse> WarehouseRepository { get; private set; }
        public IBaseRepository<StockMovement> StockMovementRepository { get; private set; }
        public IBaseRepository<StockTransfer> StockTransferRepository { get; private set; }
        public IBaseRepository<PriceList> PriceListRepository { get; private set; }
        public IBaseRepository<ProductPrice> ProductPriceRepository { get; private set; }
        public IBaseRepository<ProductBatch> ProductBatchRepository { get; private set; }

        public InventoryUnitOfWork(InventoryDbContext dbContext)
        {
            _dbContext = dbContext;
            ProductRepository = new ProductRepository(_dbContext);
            StockBalanceRepository = new StockBalanceRepository(_dbContext);

            ProductBarcodeRepository = new BaseRepository<ProductBarcode>(_dbContext);
            CategoryRepository = new BaseRepository<Category>(_dbContext);
            BrandRepository = new BaseRepository<Brand>(_dbContext);
            UnitRepository = new BaseRepository<UnitMeasure>(_dbContext);
            WarehouseRepository = new BaseRepository<Warehouse>(_dbContext);
            StockMovementRepository = new BaseRepository<StockMovement>(_dbContext);
            StockTransferRepository = new BaseRepository<StockTransfer>(_dbContext);
            PriceListRepository = new BaseRepository<PriceList>(_dbContext);
            ProductPriceRepository = new BaseRepository<ProductPrice>(_dbContext);
            ProductBatchRepository = new BaseRepository<ProductBatch>(_dbContext);
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
