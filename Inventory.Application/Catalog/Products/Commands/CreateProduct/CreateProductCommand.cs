using POS.Shared.Application.Messaging;

namespace Inventory.Application.Catalog.Products.Commands.CreateProduct
{
    public sealed record CreateProductCommand(
        string Name,
        string Sku,
        decimal Price,
        string Currency,
        int LowStockThreshold,
        Guid? CategoryId,
        Guid? BrandId,
        Guid? UnitId) : ICommand<Guid>;
}
