namespace Returns.Application.PurchaseReturns.Queries
{
    public sealed record PurchaseReturnItemResponse(
        Guid Id,
        Guid ProductId,
        string? ProductName,
        decimal Quantity,
        decimal UnitCost,
        decimal Tax,
        decimal Total);

    public sealed record PurchaseReturnResponse(
        Guid Id,
        string ReturnNumber,
        Guid OriginalPurchaseId,
        Guid SupplierId,
        string? SupplierName,
        DateTime ReturnDate,
        decimal SubTotal,
        decimal TaxAmount,
        decimal TotalAmount,
        string? Reason,
        string? Notes,
        string Status,
        Guid CreatedByUserId,
        IReadOnlyList<PurchaseReturnItemResponse> Items);
}
