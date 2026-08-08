using POS.Shared.Application.Messaging;

namespace Inventory.Application.Catalog.Categories.Commands.CreateCategory
{
    public sealed record CreateCategoryCommand(string Name, string? Description = null) : ICommand<Guid>;
}
