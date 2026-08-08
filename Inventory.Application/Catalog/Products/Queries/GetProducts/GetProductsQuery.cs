using POS.Shared.Application.Messaging;

namespace Inventory.Application.Catalog.Products.Queries.GetProducts
{
    public sealed record GetProductsQuery(
        string? SearchTerm = null,
        Guid? CategoryId = null,
        Guid? BrandId = null,
        bool? OnlyActive = true,
        int PageNumber = 1,
        int PageSize = 20) : IQuery<IReadOnlyList<ProductResponse>>;
}
