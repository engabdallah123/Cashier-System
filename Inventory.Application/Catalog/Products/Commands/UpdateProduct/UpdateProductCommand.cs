using POS.Shared.Application.Messaging;

namespace Inventory.Application.Catalog.Products.Commands.UpdateProduct
{
    public sealed record UpdateProductCommand(
        Guid Id,
        string Name,
        decimal Price,
        string Currency,
        int LowStockThreshold,
        Guid? CategoryId,
        Guid? BrandId,
        Guid? UnitId) : ICommand;
}
