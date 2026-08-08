using POS.Shared.Application.Messaging;

namespace Inventory.Application.Catalog.Units.Commands.CreateUnit
{
    public sealed record CreateUnitCommand(string Name, string Abbreviation) : ICommand<Guid>;
}
