namespace Inventory.Application.Catalog.Products.Queries
{
    public sealed record ProductResponse(
        Guid Id,
        string Barcode,
        string NameAr,
        string NameEn,
        string? Description,
        Guid CategoryId,
        string? CategoryName,
        Guid UnitId,
        string? UnitName,
        Guid? SupplierId,
        string? SupplierName,
        decimal PurchasePrice,
        decimal SellingPrice,
        decimal WholesalePrice,
        decimal QuantityInStock,
        decimal ReorderLevel,
        decimal MaxStockLevel,
        bool IsWeighable,
        bool IsActive,
        bool TrackExpiry,
        decimal TaxRate,
        string? ImageUrl,
        DateTime CreatedAt,
        DateTime? UpdatedAt);
}
