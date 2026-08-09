using POS.Shared.Application.Messaging;

namespace Inventory.Application.Catalog.Categories.Commands.CreateCategory
{
    public sealed record CreateCategoryCommand(
        string NameAr,
        string NameEn,
        Guid? ParentCategoryId = null) : ICommand<Guid>;
}
