using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Sales.Application.Sales.Queries;

namespace Sales.Application.Sales.Queries.GetSalePdf
{
    public class InvoicePdfDocument : IDocument
    {
        private readonly ReceiptResponse _receipt;

        public InvoicePdfDocument(ReceiptResponse receipt)
        {
            _receipt = receipt;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container
                .Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(36);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                    page.Header().Element(ComposeHeader);
                    page.Content().Element(ComposeContent);
                    page.Footer().Element(ComposeFooter);
                });
        }

        private void ComposeHeader(IContainer container)
        {
            var titleStyle = TextStyle.Default.FontSize(20).Bold().FontColor(Colors.Blue.Darken3);

            container.Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item().Text(_receipt.StoreName).Style(titleStyle);
                    if (!string.IsNullOrWhiteSpace(_receipt.Address))
                        column.Item().Text(_receipt.Address).FontSize(9).FontColor(Colors.Grey.Medium);
                    if (!string.IsNullOrWhiteSpace(_receipt.Phone))
                        column.Item().Text($"Tel: {_receipt.Phone}").FontSize(9).FontColor(Colors.Grey.Medium);
                });

                row.ConstantItem(200).Column(column =>
                {
                    column.Item().AlignRight().Text("SALES INVOICE").FontSize(16).Bold().FontColor(Colors.Blue.Darken2);
                    column.Item().AlignRight().Text($"Invoice #: {_receipt.InvoiceNumber}").FontSize(10).SemiBold();
                    column.Item().AlignRight().Text($"Date: {_receipt.SaleDate:yyyy-MM-dd HH:mm}").FontSize(9).FontColor(Colors.Grey.Darken1);
                });
            });
        }

        private void ComposeContent(IContainer container)
        {
            container.PaddingVertical(15).Column(column =>
            {
                // Customer & Cashier Details
                column.Item().Row(row =>
                {
                    row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Background(Colors.Grey.Lighten4).Padding(8).Column(c =>
                    {
                        c.Item().Text("CUSTOMER DETAILS").FontSize(8).Bold().FontColor(Colors.Grey.Darken1);
                        c.Item().Text(_receipt.CustomerName ?? "Walk-in Customer").FontSize(10).Bold();
                    });

                    row.ConstantItem(15);

                    row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Background(Colors.Grey.Lighten4).Padding(8).Column(c =>
                    {
                        c.Item().Text("SALE DETAILS").FontSize(8).Bold().FontColor(Colors.Grey.Darken1);
                        c.Item().Text($"Cashier: {_receipt.CashierName}").FontSize(9);
                        c.Item().Text($"Payment Method: {_receipt.PaymentMethod}").FontSize(9);
                    });
                });

                column.Item().Height(15);

                // Items Table
                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(25);  // #
                        columns.RelativeColumn(4);   // Item
                        columns.RelativeColumn(1);   // Qty
                        columns.RelativeColumn(2);   // Price
                        columns.RelativeColumn(2);   // Total
                    });

                    table.Header(header =>
                    {
                        header.Cell().Element(HeaderCellStyle).Text("#");
                        header.Cell().Element(HeaderCellStyle).Text("Item Description");
                        header.Cell().Element(HeaderCellStyle).AlignRight().Text("Qty");
                        header.Cell().Element(HeaderCellStyle).AlignRight().Text("Unit Price");
                        header.Cell().Element(HeaderCellStyle).AlignRight().Text("Total");

                        static IContainer HeaderCellStyle(IContainer c) =>
                            c.Background(Colors.Blue.Darken3).PaddingVertical(5).PaddingHorizontal(8).DefaultTextStyle(x => x.Bold().FontColor(Colors.White).FontSize(9));
                    });

                    int index = 1;
                    foreach (var item in _receipt.Items)
                    {
                        var bg = index % 2 == 0 ? Colors.Grey.Lighten4 : Colors.White;

                        table.Cell().Element(c => CellStyle(c, bg)).Text(index.ToString());
                        table.Cell().Element(c => CellStyle(c, bg)).Column(col =>
                        {
                            col.Item().Text(item.ProductName ?? "Item").Bold();
                            if (!string.IsNullOrWhiteSpace(item.Barcode))
                                col.Item().Text($"Barcode: {item.Barcode}").FontSize(8).FontColor(Colors.Grey.Darken1);
                        });
                        table.Cell().Element(c => CellStyle(c, bg)).AlignRight().Text($"{item.Quantity:N2}");
                        table.Cell().Element(c => CellStyle(c, bg)).AlignRight().Text($"{item.UnitPrice:N2} {_receipt.Currency}");
                        table.Cell().Element(c => CellStyle(c, bg)).AlignRight().Text($"{item.Total:N2} {_receipt.Currency}").Bold();

                        index++;
                    }

                    static IContainer CellStyle(IContainer c, string backgroundColor) =>
                        c.Background(backgroundColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten3).PaddingVertical(6).PaddingHorizontal(8).DefaultTextStyle(x => x.FontSize(9));
                });

                column.Item().Height(15);

                // Totals Summary Box
                column.Item().Row(row =>
                {
                    row.RelativeItem(); // Left spacer

                    row.ConstantItem(250).Column(c =>
                    {
                        c.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(col =>
                        {
                            col.Item().Row(r =>
                            {
                                r.RelativeItem().Text("Subtotal:");
                                r.ConstantItem(100).AlignRight().Text($"{_receipt.SubTotal:N2} {_receipt.Currency}");
                            });

                            if (_receipt.DiscountAmount > 0)
                            {
                                col.Item().Row(r =>
                                {
                                    r.RelativeItem().Text("Discount:").FontColor(Colors.Red.Medium);
                                    r.ConstantItem(100).AlignRight().Text($"-{_receipt.DiscountAmount:N2} {_receipt.Currency}").FontColor(Colors.Red.Medium);
                                });
                            }

                            if (_receipt.TaxAmount > 0)
                            {
                                col.Item().Row(r =>
                                {
                                    r.RelativeItem().Text("Tax:");
                                    r.ConstantItem(100).AlignRight().Text($"+{_receipt.TaxAmount:N2} {_receipt.Currency}");
                                });
                            }

                            col.Item().PaddingVertical(4).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);

                            col.Item().Row(r =>
                            {
                                r.RelativeItem().Text("Grand Total:").FontSize(11).Bold();
                                r.ConstantItem(100).AlignRight().Text($"{_receipt.TotalAmount:N2} {_receipt.Currency}").FontSize(11).Bold().FontColor(Colors.Blue.Darken3);
                            });

                            col.Item().PaddingVertical(4).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);

                            col.Item().Row(r =>
                            {
                                r.RelativeItem().Text("Paid Amount:").FontSize(9);
                                r.ConstantItem(100).AlignRight().Text($"{_receipt.PaidAmount:N2} {_receipt.Currency}").FontSize(9);
                            });

                            col.Item().Row(r =>
                            {
                                r.RelativeItem().Text("Change Due:").FontSize(9);
                                r.ConstantItem(100).AlignRight().Text($"{_receipt.ChangeAmount:N2} {_receipt.Currency}").FontSize(9);
                            });
                        });
                    });
                });
            });
        }

        private void ComposeFooter(IContainer container)
        {
            container.Column(column =>
            {
                column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                column.Item().PaddingTop(5).Row(row =>
                {
                    if (!string.IsNullOrWhiteSpace(_receipt.InvoiceFooterMessage))
                    {
                        row.RelativeItem().Text(_receipt.InvoiceFooterMessage).FontSize(9).Italic().FontColor(Colors.Grey.Darken1);
                    }
                    else
                    {
                        row.RelativeItem().Text("Thank you for your business!").FontSize(9).Italic().FontColor(Colors.Grey.Darken1);
                    }

                    row.ConstantItem(120).AlignRight().Text(x =>
                    {
                        x.Span("Page ").FontSize(8).FontColor(Colors.Grey.Medium);
                        x.CurrentPageNumber().FontSize(8).FontColor(Colors.Grey.Medium);
                        x.Span(" of ").FontSize(8).FontColor(Colors.Grey.Medium);
                        x.TotalPages().FontSize(8).FontColor(Colors.Grey.Medium);
                    });
                });
            });
        }
    }
}
