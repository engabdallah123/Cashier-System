using Inventory.Domain.Catalog.Products.Errors;
using POS.Shared.Domain;

namespace Inventory.Domain.Catalog.Products.Entities
{
    public sealed class Product : Entity
    {
        public string Barcode { get; private set; } = default!;
        public string NameAr { get; private set; } = default!;
        public string NameEn { get; private set; } = default!;
        public string? Description { get; private set; }

        public Guid CategoryId { get; private set; }
        public Guid UnitId { get; private set; }
        public Guid? SupplierId { get; private set; }

        public decimal PurchasePrice { get; private set; }
        public decimal SellingPrice { get; private set; }
        public decimal WholesalePrice { get; private set; }

        public decimal QuantityInStock { get; private set; }
        public decimal ReorderLevel { get; private set; }
        public decimal MaxStockLevel { get; private set; }

        public bool IsWeighable { get; private set; }
        public bool IsActive { get; private set; }
        public bool TrackExpiry { get; private set; }
        public decimal TaxRate { get; private set; }
        public string? ImageUrl { get; private set; }

        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }

        private Product() { } // EF Core

        private Product(
            Guid id, string barcode, string nameAr, string nameEn, string? description,
            Guid categoryId, Guid unitId, Guid? supplierId,
            decimal purchasePrice, decimal sellingPrice, decimal wholesalePrice,
            decimal reorderLevel, decimal maxStockLevel,
            bool isWeighable, bool isActive, bool trackExpiry, decimal taxRate, string? imageUrl)
            : base(id)
        {
            Barcode = barcode;
            NameAr = nameAr;
            NameEn = nameEn;
            Description = description;
            CategoryId = categoryId;
            UnitId = unitId;
            SupplierId = supplierId;
            PurchasePrice = purchasePrice;
            SellingPrice = sellingPrice;
            WholesalePrice = wholesalePrice;
            QuantityInStock = 0;
            ReorderLevel = reorderLevel;
            MaxStockLevel = maxStockLevel;
            IsWeighable = isWeighable;
            IsActive = isActive;
            TrackExpiry = trackExpiry;
            TaxRate = taxRate;
            ImageUrl = imageUrl;
            CreatedAt = DateTime.UtcNow;
        }

        public static Result<Product> Create(
            string barcode, string nameAr, string nameEn, Guid categoryId, Guid unitId,
            decimal purchasePrice, decimal sellingPrice, decimal wholesalePrice = 0,
            Guid? supplierId = null, string? description = null,
            decimal reorderLevel = 5, decimal maxStockLevel = 100,
            bool isWeighable = false, bool isActive = true, bool trackExpiry = false,
            decimal taxRate = 0, string? imageUrl = null)
        {
            if (string.IsNullOrWhiteSpace(barcode))
                return Result<Product>.Failure(ProductErrors.BarcodeRequired);

            if (string.IsNullOrWhiteSpace(nameAr))
                return Result<Product>.Failure(ProductErrors.NameArRequired);

            if (string.IsNullOrWhiteSpace(nameEn))
                return Result<Product>.Failure(ProductErrors.NameEnRequired);

            if (purchasePrice < 0)
                return Result<Product>.Failure(ProductErrors.InvalidPurchasePrice);

            if (sellingPrice < 0)
                return Result<Product>.Failure(ProductErrors.InvalidSellingPrice);

            var product = new Product(
                Guid.NewGuid(), barcode.Trim(), nameAr.Trim(), nameEn.Trim(), description?.Trim(),
                categoryId, unitId, supplierId,
                purchasePrice, sellingPrice, wholesalePrice,
                reorderLevel, maxStockLevel,
                isWeighable, isActive, trackExpiry, taxRate, imageUrl?.Trim());

            return Result<Product>.Success(product);
        }

        public Result Update(
            string barcode, string nameAr, string nameEn, string? description,
            Guid categoryId, Guid unitId, Guid? supplierId,
            decimal purchasePrice, decimal sellingPrice, decimal wholesalePrice,
            decimal reorderLevel, decimal maxStockLevel,
            bool isWeighable, bool isActive, bool trackExpiry, decimal taxRate, string? imageUrl)
        {
            if (string.IsNullOrWhiteSpace(barcode))
                return Result.Failure(ProductErrors.BarcodeRequired);

            if (string.IsNullOrWhiteSpace(nameAr))
                return Result.Failure(ProductErrors.NameArRequired);

            if (string.IsNullOrWhiteSpace(nameEn))
                return Result.Failure(ProductErrors.NameEnRequired);

            Barcode = barcode.Trim();
            NameAr = nameAr.Trim();
            NameEn = nameEn.Trim();
            Description = description?.Trim();
            CategoryId = categoryId;
            UnitId = unitId;
            SupplierId = supplierId;
            PurchasePrice = purchasePrice;
            SellingPrice = sellingPrice;
            WholesalePrice = wholesalePrice;
            ReorderLevel = reorderLevel;
            MaxStockLevel = maxStockLevel;
            IsWeighable = isWeighable;
            IsActive = isActive;
            TrackExpiry = trackExpiry;
            TaxRate = taxRate;
            ImageUrl = imageUrl?.Trim();
            UpdatedAt = DateTime.UtcNow;

            return Result.Success();
        }

        public Result AdjustStock(decimal delta, bool allowNegativeStock = false)
        {
            var newQuantity = QuantityInStock + delta;

            if (!allowNegativeStock && newQuantity < 0)
                return Result.Failure(ProductErrors.InsufficientStock);

            QuantityInStock = newQuantity;
            UpdatedAt = DateTime.UtcNow;

            return Result.Success();
        }

        public bool IsLowStock() => QuantityInStock <= ReorderLevel;
    }
}
