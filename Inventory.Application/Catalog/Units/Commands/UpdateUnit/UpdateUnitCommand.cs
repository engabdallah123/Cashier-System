using POS.Shared.Application.Messaging;

namespace Inventory.Application.Catalog.Units.Commands.UpdateUnit
{
    public sealed record UpdateUnitCommand(Guid Id, string Name, string Abbreviation) : ICommand;
}
