using Inventory.Domain.Batches.ProductBatches;
using Inventory.Domain.Catalog.Brands;
using Inventory.Domain.Catalog.Categories;
using Inventory.Domain.Catalog.ProductBarcodes;
using Inventory.Domain.Catalog.Products.Entities;
using Inventory.Domain.Catalog.Products.Interface;
using Inventory.Domain.Catalog.Units;
using Inventory.Domain.Pricing.PriceLists;
using Inventory.Domain.Pricing.ProductPrices;
using Inventory.Domain.Stock.StockBalances;
using Inventory.Domain.Stock.StockMovements;
using Inventory.Domain.Stock.StockTransfers;
using Inventory.Domain.Stock.Warehouses;
using POS.Shared.Domain;
using POS.Shared.Domain.Abstractions;

namespace Inventory.Domain
{
    public interface IInventoryUnitOfWork : IUnitOfWork
    {
        IProductRepository ProductRepository { get; }
        IStockBalanceRepository StockBalanceRepository { get; }

        IBaseRepository<ProductBarcode> ProductBarcodeRepository { get; }
        IBaseRepository<Category> CategoryRepository { get; }
        IBaseRepository<Brand> BrandRepository { get; }
        IBaseRepository<UnitMeasure> UnitRepository { get; }
        IBaseRepository<Warehouse> WarehouseRepository { get; }
        IBaseRepository<StockMovement> StockMovementRepository { get; }
        IBaseRepository<StockTransfer> StockTransferRepository { get; }
        IBaseRepository<PriceList> PriceListRepository { get; }
        IBaseRepository<ProductPrice> ProductPriceRepository { get; }
        IBaseRepository<ProductBatch> ProductBatchRepository { get; }
    }
}