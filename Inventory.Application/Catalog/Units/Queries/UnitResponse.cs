namespace Inventory.Application.Catalog.Units.Queries
{
    public sealed record UnitResponse(
        Guid Id,
        string Name,
        string Abbreviation,
        bool IsActive,
        DateTime CreatedAt);
}
