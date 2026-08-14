using ClosedXML.Excel;
using POS.Shared.Application.Messaging;
using POS.Shared.Domain;

namespace Inventory.Application.Catalog.Products.Queries.GetProductExcelTemplate
{
    internal sealed class GetProductExcelTemplateQueryHandler : IQueryHandler<GetProductExcelTemplateQuery, byte[]>
    {
        public Task<Result<byte[]>> Handle(GetProductExcelTemplateQuery request, CancellationToken cancellationToken)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("المنتجات");
            worksheet.RightToLeft = true;

            // Header Column Names
            string[] headers = new[]
            {
                "الباركود*",
                "اسم المنتج (عربي)*",
                "اسم المنتج (إنجليزي)",
                "التصنيف",
                "الوحدة",
                "سعر الشراء",
                "سعر البيع*",
                "سعر الجملة",
                "الرصيد الأولي",
                "حد إعادة الطلب",
                "الحد الأقصى للمخزون",
                "نسبة الضريبة %",
                "قابل للوزن (نعم/لا)",
                "تاريخ الانتهاء (نعم/لا)",
                "رابط الصورة (URL)",
                "الوصف"
            };

            // Write Headers
            for (int col = 0; col < headers.Length; col++)
            {
                var cell = worksheet.Cell(1, col + 1);
                cell.Value = headers[col];
                cell.Style.Font.Bold = true;
                cell.Style.Font.FontColor = XLColor.White;
                cell.Style.Fill.BackgroundColor = XLColor.FromArgb(30, 41, 59); // Dark slate
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            }

            // Write Sample Row 1
            worksheet.Cell(2, 1).SetValue("6221234567890");
            worksheet.Cell(2, 2).SetValue("شيبسي طماطم 50جم");
            worksheet.Cell(2, 3).SetValue("Chipsy Tomato 50g");
            worksheet.Cell(2, 4).SetValue("مأكولات وخفيفات");
            worksheet.Cell(2, 5).SetValue("قطعة");
            worksheet.Cell(2, 6).SetValue(8.00);
            worksheet.Cell(2, 7).SetValue(10.00);
            worksheet.Cell(2, 8).SetValue(9.50);
            worksheet.Cell(2, 9).SetValue(50);
            worksheet.Cell(2, 10).SetValue(10);
            worksheet.Cell(2, 11).SetValue(200);
            worksheet.Cell(2, 12).SetValue(0);
            worksheet.Cell(2, 13).SetValue("لا");
            worksheet.Cell(2, 14).SetValue("نعم");
            worksheet.Cell(2, 15).SetValue("https://images.openfoodfacts.org/images/products/622/123/456/7890/1.jpg");
            worksheet.Cell(2, 16).SetValue("شيبس طماطم الحجم العائلي");

            // Write Sample Row 2
            worksheet.Cell(3, 1).SetValue("6229876543210");
            worksheet.Cell(3, 2).SetValue("موز فريش (بالكيلو)");
            worksheet.Cell(3, 3).SetValue("Fresh Bananas (Kg)");
            worksheet.Cell(3, 4).SetValue("فواكه وخضروات");
            worksheet.Cell(3, 5).SetValue("كيلو");
            worksheet.Cell(3, 6).SetValue(25.00);
            worksheet.Cell(3, 7).SetValue(35.00);
            worksheet.Cell(3, 8).SetValue(30.00);
            worksheet.Cell(3, 9).SetValue(100);
            worksheet.Cell(3, 10).SetValue(15);
            worksheet.Cell(3, 11).SetValue(500);
            worksheet.Cell(3, 12).SetValue(0);
            worksheet.Cell(3, 13).SetValue("نعم");
            worksheet.Cell(3, 14).SetValue("لا");
            worksheet.Cell(3, 15).SetValue("");
            worksheet.Cell(3, 16).SetValue("موز بلدي فاخر");

            worksheet.Columns().AdjustToContents();

            using var memoryStream = new MemoryStream();
            workbook.SaveAs(memoryStream);

            return Task.FromResult(Result<byte[]>.Success(memoryStream.ToArray()));
        }
    }
}
