using POS.Shared.Application.Messaging;

namespace Inventory.Application.Catalog.Categories.Commands.UpdateCategory
{
    public sealed record UpdateCategoryCommand(
        Guid Id,
        string NameAr,
        string NameEn,
        Guid? ParentCategoryId) : ICommand;
}
