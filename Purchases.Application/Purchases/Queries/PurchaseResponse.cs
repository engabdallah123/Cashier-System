namespace Purchases.Application.Purchases.Queries
{
    public sealed record PurchaseItemResponse(
        Guid Id,
        Guid ProductId,
        string? ProductName,
        decimal Quantity,
        decimal UnitCost,
        decimal Discount,
        decimal Tax,
        decimal Total,
        DateTime? ExpiryDate,
        string? BatchNumber);

    public sealed record PurchaseResponse(
        Guid Id,
        string InvoiceNumber,
        string? InternalNumber,
        Guid SupplierId,
        string? SupplierName,
        DateTime PurchaseDate,
        DateTime? ReceivedDate,
        decimal SubTotal,
        decimal DiscountAmount,
        decimal TaxAmount,
        decimal TotalAmount,
        decimal PaidAmount,
        decimal RemainingAmount,
        string PaymentMethod,
        string Status,
        string? Notes,
        Guid CreatedByUserId,
        IReadOnlyList<PurchaseItemResponse> Items);
}
