using Inventory.Domain.Catalog.Products.Errors;
using Inventory.Domain.Catalog.Products.Events;
using POS.Shared.Domain;
using POS.Shared.Domain.Events.Inventory;

namespace Inventory.Domain.Catalog.Products.Entities
{
    public sealed class Product : Entity
    {
        public string Name { get; private set; } = default!;
        public Sku Sku { get; private set; } = default!;
        public Money Price { get; private set; } = default!;
        public int QuantityOnHand { get; private set; }
        public int LowStockThreshold { get; private set; }
        public bool IsActive { get; private set; } = true;
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; private set; }

        public Guid? CategoryId { get; private set; }
        public Guid? BrandId { get; private set; }
        public Guid? UnitId { get; private set; }

        private Product() { } // EF Core

        private Product(Guid id, string name, Sku sku, Money price, int lowStockThreshold, bool isActive
            , DateTime createdAt, DateTime? updatedAt, Guid? categoryId, Guid? brandId, Guid? unitId)
            : base(id)
        {
            Name = name;
            Sku = sku;
            Price = price;
            LowStockThreshold = lowStockThreshold;
            IsActive = isActive;
            CreatedAt = createdAt;
            UpdatedAt = updatedAt;
            QuantityOnHand = 0;
            CategoryId = categoryId;
            BrandId = brandId;
            UnitId = unitId;
        }

        public static Result<Product> Create(
            string name, Sku sku, Money price, int lowStockThreshold = 5, bool isActive = true, DateTime? updatedAt = null, Guid? categoryId = null, Guid? brandId = null, Guid? unitId = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                return Result<Product>.Failure(ProductErrors.NameRequired);

            if (lowStockThreshold < 0)
                return Result<Product>.Failure(ProductErrors.LowStockThresholdInvalid);

            var product = new Product(Guid.NewGuid(), name.Trim(), sku, price, lowStockThreshold, isActive, DateTime.UtcNow, updatedAt, categoryId, brandId, unitId);

            product.RaiseDomainEvent(new ProductCreatedDomainEvent(product.Id, sku.Value));

            return Result<Product>.Success(product);
        }

        public Result UpdateInfo(string name, Money price, int lowStockThreshold, Guid? categoryId, Guid? brandId, Guid? unitId)
        {
            if (string.IsNullOrWhiteSpace(name))
                return Result.Failure(ProductErrors.NameRequired);

            if (lowStockThreshold < 0)
                return Result.Failure(ProductErrors.LowStockThresholdInvalid);

            Name = name.Trim();
            Price = price;
            LowStockThreshold = lowStockThreshold;
            CategoryId = categoryId;
            BrandId = brandId;
            UnitId = unitId;
            UpdatedAt = DateTime.UtcNow;

            return Result.Success();
        }

        public Result AdjustStock(int delta)
        {
            var newQuantity = QuantityOnHand + delta;

            if (newQuantity < 0)
                return Result.Failure(ProductErrors.InsufficientStock);

            QuantityOnHand = newQuantity;

            RaiseDomainEvent(new ProductStockChangedIntegrationEvent(Id, Sku.Value, QuantityOnHand, IsLowStock()));

            return Result.Success();
        }

        public bool IsLowStock() => QuantityOnHand <= LowStockThreshold;
        public bool IsOutOfStock() => QuantityOnHand == 0;

        public Result Activate()
        {
            IsActive = true;
            UpdatedAt = DateTime.UtcNow;
            return Result.Success();
        }

        public Result Deactivate()
        {
            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
            return Result.Success();
        }
    }
}
