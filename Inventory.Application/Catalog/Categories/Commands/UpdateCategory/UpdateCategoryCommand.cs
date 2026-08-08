using POS.Shared.Application.Messaging;

namespace Inventory.Application.Catalog.Categories.Commands.UpdateCategory
{
    public sealed record UpdateCategoryCommand(
        Guid Id,
        string Name,
        string? Description) : ICommand;
}
