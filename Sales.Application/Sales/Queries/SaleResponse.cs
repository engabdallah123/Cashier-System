namespace Sales.Application.Sales.Queries
{
    public sealed record SaleItemResponse(
        Guid Id,
        Guid ProductId,
        string? ProductName,
        string? Barcode,
        decimal Quantity,
        decimal UnitPrice,
        decimal Discount,
        decimal Tax,
        decimal Total);

    public sealed record SaleResponse(
        Guid Id,
        string InvoiceNumber,
        DateTime SaleDate,
        Guid CashierId,
        string? CashierName,
        Guid? CustomerId,
        string? CustomerName,
        Guid ShiftId,
        decimal SubTotal,
        decimal DiscountAmount,
        decimal TaxAmount,
        decimal TotalAmount,
        decimal PaidAmount,
        decimal ChangeAmount,
        string PaymentMethod,
        string Status,
        string? Notes,
        IReadOnlyList<SaleItemResponse> Items);

    public sealed record ReceiptResponse(
        string StoreName,
        string? Address,
        string? Phone,
        string InvoiceNumber,
        DateTime SaleDate,
        string CashierName,
        string? CustomerName,
        IReadOnlyList<SaleItemResponse> Items,
        decimal SubTotal,
        decimal DiscountAmount,
        decimal TaxAmount,
        decimal TotalAmount,
        decimal PaidAmount,
        decimal ChangeAmount,
        string PaymentMethod,
        string Currency,
        string? InvoiceFooterMessage);
}
