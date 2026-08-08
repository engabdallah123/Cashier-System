using Inventory.Domain.Catalog.Products.Entities;

namespace Inventory.Domain.Catalog.Products.Interface
{
    public interface IProductRepository
    {
        Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        Task<Product?> GetByBarcodeAsync(string barcode, CancellationToken cancellationToken = default);

        Task<bool> SkuExistsAsync(string sku, CancellationToken cancellationToken = default);

        Task<bool> BarcodeExistsAsync(string barcode, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<Product>> GetLowStockAsync(CancellationToken cancellationToken = default);

        Task AddAsync(Product product, CancellationToken cancellationToken = default);

        void Update(Product product);

        void Remove(Product product);

        Task<IReadOnlyList<Product>> GetAllActiveAsync(CancellationToken cancellationToken = default);
    }
}
