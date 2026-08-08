namespace Inventory.Application.Catalog.Products.Queries
{
    public sealed record ProductResponse(
        Guid Id,
        string Name,
        string Sku,
        decimal Price,
        string Currency,
        int QuantityOnHand,
        int LowStockThreshold,
        bool IsActive,
        Guid? CategoryId,
        string? CategoryName,
        Guid? BrandId,
        string? BrandName,
        Guid? UnitId,
        string? UnitName,
        DateTime CreatedAt,
        DateTime? UpdatedAt);
}
