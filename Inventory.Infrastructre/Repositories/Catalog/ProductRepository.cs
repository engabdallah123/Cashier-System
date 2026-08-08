using Inventory.Domain.Catalog.Products.Entities;
using Inventory.Domain.Catalog.Products.Interface;
using Inventory.Infrastructre.Database;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Infrastructre.Repositories.Catalog
{
    public class ProductRepository : IProductRepository
    {
        private readonly InventoryDbContext _context;

        public ProductRepository(InventoryDbContext context)
        {
            _context = context;
        }

        public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }

        public async Task<Product?> GetByBarcodeAsync(string barcode, CancellationToken cancellationToken = default)
        {
            var productBarcode = await _context.ProductBarcodes
                .FirstOrDefaultAsync(pb => pb.Barcode == barcode, cancellationToken);

            if (productBarcode is null)
                return null;

            return await _context.Products
                .FirstOrDefaultAsync(p => p.Id == productBarcode.ProductId, cancellationToken);
        }

        public async Task<bool> SkuExistsAsync(string sku, CancellationToken cancellationToken = default)
        {
            return await _context.Products
                .AnyAsync(p => p.Sku.Value == sku, cancellationToken);
        }

        public async Task<bool> BarcodeExistsAsync(string barcode, CancellationToken cancellationToken = default)
        {
            return await _context.ProductBarcodes
                .AnyAsync(pb => pb.Barcode == barcode, cancellationToken);
        }

        public async Task<IReadOnlyList<Product>> GetLowStockAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Products
                .Where(p => p.IsActive && p.QuantityOnHand <= p.LowStockThreshold)
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(Product product, CancellationToken cancellationToken = default)
        {
            await _context.Products.AddAsync(product, cancellationToken);
        }

        public void Update(Product product)
        {
            _context.Products.Update(product);
        }

        public void Remove(Product product)
        {
            _context.Products.Remove(product);
        }

        public async Task<IReadOnlyList<Product>> GetAllActiveAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Products
                .Where(p => p.IsActive)
                .ToListAsync(cancellationToken);
        }
    }
}
