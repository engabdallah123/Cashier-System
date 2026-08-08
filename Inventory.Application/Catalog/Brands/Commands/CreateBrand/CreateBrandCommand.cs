using POS.Shared.Application.Messaging;

namespace Inventory.Application.Catalog.Brands.Commands.CreateBrand
{
    public sealed record CreateBrandCommand(string Name) : ICommand<Guid>;
}
