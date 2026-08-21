namespace Purchases.Application.Purchases.Queries.GetPurchaseById
{
    public sealed record PurchaseDetailItemResponse(
        Guid Id,
        Guid ProductId,
        string? ProductName,
        string? Barcode,
        decimal Quantity,
        decimal UnitCost,
        decimal Discount,
        decimal Tax,
        decimal Total,
        DateTime? ExpiryDate,
        string? BatchNumber);

    public sealed record PurchaseDetailResponse(
        Guid Id,
        string InvoiceNumber,
        string? InternalNumber,
        DateTime PurchaseDate,
        Guid SupplierId,
        string? SupplierName,
        decimal SubTotal,
        decimal DiscountAmount,
        decimal TaxAmount,
        decimal TotalAmount,
        decimal PaidAmount,
        decimal RemainingAmount,
        string Status,
        string? Notes,
        IReadOnlyList<PurchaseDetailItemResponse> Items);
}
