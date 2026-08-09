using POS.Shared.Application.Messaging;

namespace Sales.Application.Sales.Queries.GetSalePdf
{
    public sealed record GetSalePdfQuery(Guid SaleId) : IQuery<byte[]>;
}
