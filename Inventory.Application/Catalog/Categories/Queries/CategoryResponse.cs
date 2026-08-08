namespace Inventory.Application.Catalog.Categories.Queries
{
    public sealed record CategoryResponse(
        Guid Id,
        string Name,
        string? Description,
        bool IsActive,
        DateTime CreatedAt);
}
