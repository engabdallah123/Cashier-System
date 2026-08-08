using POS.Shared.Application.Messaging;

namespace Inventory.Application.Catalog.Brands.Commands.UpdateBrand
{
    public sealed record UpdateBrandCommand(Guid Id, string Name) : ICommand;
}
