using ClosedXML.Excel;
using Inventory.Domain;
using Inventory.Domain.Catalog.Categories;
using Inventory.Domain.Catalog.Products.Entities;
using Inventory.Domain.Catalog.Units;
using Microsoft.AspNetCore.Http;
using POS.Shared.Application.IService;
using POS.Shared.Application.Messaging;
using POS.Shared.Domain;
using System.IO.Compression;

namespace Inventory.Application.Catalog.Products.Commands.ImportProductsFromExcel
{
    internal sealed class ImportProductsFromExcelCommandHandler
        : ICommandHandler<ImportProductsFromExcelCommand, ProductImportResultDto>
    {
        private readonly IInventoryUnitOfWork _unitOfWork;
        private readonly IFileService _fileService;

        public ImportProductsFromExcelCommandHandler(
            IInventoryUnitOfWork unitOfWork,
            IFileService fileService)
        {
            _unitOfWork = unitOfWork;
            _fileService = fileService;
        }

        public async Task<Result<ProductImportResultDto>> Handle(
            ImportProductsFromExcelCommand request,
            CancellationToken cancellationToken)
        {
            var result = new ProductImportResultDto();

            if (request.FileBytes == null || request.FileBytes.Length == 0)
            {
                return Result<ProductImportResultDto>.Failure(
                    new Error("Excel.EmptyFile", "ملف الإكسيل فارغ أو غير صالح."));
            }

            // 1. Detect if file is a ZIP archive containing products.xlsx + images/ folder
            byte[] excelBytes = request.FileBytes;
            var zipImagesByBarcode = new Dictionary<string, ZipArchiveEntry>(StringComparer.OrdinalIgnoreCase);

            if (IsZipFile(request.FileBytes))
            {
                try
                {
                    using var zipStream = new MemoryStream(request.FileBytes);
                    using var archive = new ZipArchive(zipStream, ZipArchiveMode.Read, leaveOpen: true);

                    // Find excel entry (*.xlsx or *.xls) inside ZIP archive
                    var excelEntry = archive.Entries.FirstOrDefault(e =>
                        e.FullName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase) ||
                        e.FullName.EndsWith(".xls", StringComparison.OrdinalIgnoreCase));

                    if (excelEntry != null)
                    {
                        using var ms = new MemoryStream();
                        using var entryStream = excelEntry.Open();
                        await entryStream.CopyToAsync(ms, cancellationToken);
                        excelBytes = ms.ToArray();
                    }

                    // Index image files by barcode (filename without extension)
                    foreach (var entry in archive.Entries)
                    {
                        var ext = Path.GetExtension(entry.FullName).ToLowerInvariant();
                        if (ext is ".jpg" or ".jpeg" or ".png" or ".webp" or ".gif")
                        {
                            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(entry.FullName).Trim();
                            if (!string.IsNullOrWhiteSpace(fileNameWithoutExt) && !zipImagesByBarcode.ContainsKey(fileNameWithoutExt))
                            {
                                zipImagesByBarcode[fileNameWithoutExt] = entry;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    return Result<ProductImportResultDto>.Failure(
                        new Error("Zip.Invalid", $"فشل قراءة ملف الـ ZIP المضغوط: {ex.Message}"));
                }
            }

            // 2. Pre-fetch existing Categories, Units, and Products for high-speed Bulk operations
            var existingCategories = await _unitOfWork.CategoryRepository.GetAllAsync();
            var categoryMap = new Dictionary<string, Category>(StringComparer.OrdinalIgnoreCase);
            foreach (var cat in existingCategories)
            {
                if (!string.IsNullOrWhiteSpace(cat.NameAr) && !categoryMap.ContainsKey(cat.NameAr.Trim()))
                    categoryMap[cat.NameAr.Trim()] = cat;
                if (!string.IsNullOrWhiteSpace(cat.NameEn) && !categoryMap.ContainsKey(cat.NameEn.Trim()))
                    categoryMap[cat.NameEn.Trim()] = cat;
            }

            var existingUnits = await _unitOfWork.UnitRepository.GetAllAsync();
            var unitMap = new Dictionary<string, Unit>(StringComparer.OrdinalIgnoreCase);
            foreach (var u in existingUnits)
            {
                if (!string.IsNullOrWhiteSpace(u.NameAr) && !unitMap.ContainsKey(u.NameAr.Trim()))
                    unitMap[u.NameAr.Trim()] = u;
                if (!string.IsNullOrWhiteSpace(u.NameEn) && !unitMap.ContainsKey(u.NameEn.Trim()))
                    unitMap[u.NameEn.Trim()] = u;
                if (!string.IsNullOrWhiteSpace(u.Symbol) && !unitMap.ContainsKey(u.Symbol.Trim()))
                    unitMap[u.Symbol.Trim()] = u;
            }

            var allExistingProducts = await _unitOfWork.ProductRepository.GetAllAsync();
            var productMap = allExistingProducts.ToDictionary(p => p.Barcode.Trim(), StringComparer.OrdinalIgnoreCase);

            // 3. Open Excel Workbook
            using var excelStream = new MemoryStream(excelBytes);
            using var workbook = new XLWorkbook(excelStream);
            var worksheet = workbook.Worksheets.FirstOrDefault();

            if (worksheet == null)
            {
                return Result<ProductImportResultDto>.Failure(
                    new Error("Excel.NoWorksheet", "لم يتم العثور على ورقة عمل في ملف الإكسيل."));
            }

            var range = worksheet.RangeUsed();
            if (range == null || range.RowCount() < 2)
            {
                return Result<ProductImportResultDto>.Failure(
                    new Error("Excel.NoDataRows", "لا تحتوي ورقة العمل على صفوف بيانات بعد الترويسة."));
            }

            // 4. Dynamic Column Header Detection (Row 1)
            var headerRow = worksheet.Row(1);
            int lastCellNum = Math.Max(16, headerRow.LastCellUsed()?.Address.ColumnNumber ?? 16);

            int imageColIndex = -1;
            int descColIndex = -1;

            for (int col = 1; col <= lastCellNum; col++)
            {
                var hText = GetCellValue(headerRow.Cell(col)).ToLowerInvariant();
                if (imageColIndex == -1 && (hText.Contains("صورة") || hText.Contains("صوره") || hText.Contains("image") || hText.Contains("url") || hText.Contains("رابط")))
                {
                    imageColIndex = col;
                }
                else if (descColIndex == -1 && (hText.Contains("وصف") || hText.Contains("description") || hText.Contains("تفاصيل")))
                {
                    descColIndex = col;
                }
            }

            int lastRow = range.LastRow().RowNumber();
            result.TotalRows = lastRow - 1; // Exclude header

            var batchBarcodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var newProductsToAdd = new List<Product>();
            var newCategoriesToAdd = new List<Category>();
            var newUnitsToAdd = new List<Unit>();

            for (int rowNum = 2; rowNum <= lastRow; rowNum++)
            {
                var row = worksheet.Row(rowNum);

                string barcode = GetCellValue(row.Cell(1));
                string nameAr = GetCellValue(row.Cell(2));
                string nameEn = GetCellValue(row.Cell(3));
                string categoryName = GetCellValue(row.Cell(4));
                string unitName = GetCellValue(row.Cell(5));

                decimal purchasePrice = ParseDecimal(GetCellValue(row.Cell(6)), 0);
                decimal sellingPrice = ParseDecimal(GetCellValue(row.Cell(7)), 0);
                decimal wholesalePrice = ParseDecimal(GetCellValue(row.Cell(8)), 0);
                decimal initialStock = ParseDecimal(GetCellValue(row.Cell(9)), 0);
                decimal reorderLevel = ParseDecimal(GetCellValue(row.Cell(10)), 5);
                decimal maxStockLevel = ParseDecimal(GetCellValue(row.Cell(11)), 100);
                decimal taxRate = ParseDecimal(GetCellValue(row.Cell(12)), 0);

                bool isWeighable = ParseBool(GetCellValue(row.Cell(13)));
                bool trackExpiry = ParseBool(GetCellValue(row.Cell(14)));

                string col15 = GetCellValue(row.Cell(15));
                string col16 = GetCellValue(row.Cell(16));

                string? rawImageUrl = null;
                string? description = null;

                // Priority 1: Check dynamic header column for Image URL
                if (imageColIndex > 0)
                {
                    var imgVal = GetCellValue(row.Cell(imageColIndex));
                    if (!string.IsNullOrWhiteSpace(imgVal))
                    {
                        rawImageUrl = imgVal;
                    }
                }

                // Priority 2: Check dynamic header column for Description
                if (descColIndex > 0)
                {
                    var descVal = GetCellValue(row.Cell(descColIndex));
                    if (!string.IsNullOrWhiteSpace(descVal))
                    {
                        description = descVal;
                    }
                }

                // Fallback smart detection if header matching did not yield an image URL
                if (string.IsNullOrWhiteSpace(rawImageUrl))
                {
                    if (IsPossibleImageUrl(col15))
                    {
                        rawImageUrl = col15;
                        if (string.IsNullOrWhiteSpace(description)) description = col16;
                    }
                    else if (IsPossibleImageUrl(col16))
                    {
                        rawImageUrl = col16;
                        if (string.IsNullOrWhiteSpace(description)) description = col15;
                    }
                    else
                    {
                        if (string.IsNullOrWhiteSpace(description))
                            description = !string.IsNullOrWhiteSpace(col15) ? col15 : col16;
                    }
                }

                // Basic Validations
                if (string.IsNullOrWhiteSpace(barcode))
                {
                    result.ErrorCount++;
                    result.Errors.Add(new ProductImportErrorDto
                    {
                        RowNumber = rowNum,
                        Barcode = string.Empty,
                        ProductName = nameAr,
                        ErrorMessage = "البار كود مطلوب ولا يمكن أن يكون فارغاً."
                    });
                    continue;
                }

                if (string.IsNullOrWhiteSpace(nameAr))
                {
                    result.ErrorCount++;
                    result.Errors.Add(new ProductImportErrorDto
                    {
                        RowNumber = rowNum,
                        Barcode = barcode,
                        ProductName = string.Empty,
                        ErrorMessage = "اسم المنتج بالعربية مطلوب."
                    });
                    continue;
                }

                if (sellingPrice < 0)
                {
                    result.ErrorCount++;
                    result.Errors.Add(new ProductImportErrorDto
                    {
                        RowNumber = rowNum,
                        Barcode = barcode,
                        ProductName = nameAr,
                        ErrorMessage = "سعر البيع لا يمكن أن يكون بالسالب."
                    });
                    continue;
                }

                // Check duplicates within same Excel file
                if (!batchBarcodes.Add(barcode))
                {
                    result.ErrorCount++;
                    result.Errors.Add(new ProductImportErrorDto
                    {
                        RowNumber = rowNum,
                        Barcode = barcode,
                        ProductName = nameAr,
                        ErrorMessage = "الباركود مكرر أكثر من مرة في نفس ملف الإكسيل."
                    });
                    continue;
                }

                if (string.IsNullOrWhiteSpace(nameEn))
                {
                    nameEn = nameAr;
                }

                // 1. Resolve Category
                Guid categoryId;
                string catKey = string.IsNullOrWhiteSpace(categoryName) ? "عام" : categoryName.Trim();

                if (categoryMap.TryGetValue(catKey, out var category))
                {
                    categoryId = category.Id;
                }
                else
                {
                    var newCatResult = Category.Create(catKey, catKey);
                    if (newCatResult.IsFailure)
                    {
                        result.ErrorCount++;
                        result.Errors.Add(new ProductImportErrorDto
                        {
                            RowNumber = rowNum,
                            Barcode = barcode,
                            ProductName = nameAr,
                            ErrorMessage = $"فشل إنشاء التصنيف '{catKey}': {newCatResult.Error.Name}"
                        });
                        continue;
                    }

                    var newCat = newCatResult.Value!;
                    newCategoriesToAdd.Add(newCat);
                    categoryMap[catKey] = newCat;
                    categoryId = newCat.Id;
                }

                // 2. Resolve Unit
                Guid unitId;
                string uKey = string.IsNullOrWhiteSpace(unitName) ? "قطعة" : unitName.Trim();

                if (unitMap.TryGetValue(uKey, out var unit))
                {
                    unitId = unit.Id;
                }
                else
                {
                    var newUnitResult = Unit.Create(uKey, uKey, uKey);
                    if (newUnitResult.IsFailure)
                    {
                        result.ErrorCount++;
                        result.Errors.Add(new ProductImportErrorDto
                        {
                            RowNumber = rowNum,
                            Barcode = barcode,
                            ProductName = nameAr,
                            ErrorMessage = $"فشل إنشاء الوحدة '{uKey}': {newUnitResult.Error.Name}"
                        });
                        continue;
                    }

                    var newUnit = newUnitResult.Value!;
                    newUnitsToAdd.Add(newUnit);
                    unitMap[uKey] = newUnit;
                    unitId = newUnit.Id;
                }

                // 3. Resolve Image (ZIP matched by Barcode OR Text URL string directly)
                string? resolvedImageUrl = await ProcessProductImageAsync(barcode, rawImageUrl, zipImagesByBarcode, cancellationToken);

                // 4. Resolve Product in Database
                if (productMap.TryGetValue(barcode.Trim(), out var existingProduct))
                {
                    if (!request.UpdateExisting)
                    {
                        result.ErrorCount++;
                        result.Errors.Add(new ProductImportErrorDto
                        {
                            RowNumber = rowNum,
                            Barcode = barcode,
                            ProductName = nameAr,
                            ErrorMessage = $"المنتج بالباركود '{barcode}' موجود بالفعل في الكتالوج."
                        });
                        continue;
                    }

                    string? finalImageUrl = !string.IsNullOrWhiteSpace(resolvedImageUrl)
                        ? resolvedImageUrl
                        : existingProduct.ImageUrl;

                    var updateRes = existingProduct.Update(
                        barcode, nameAr, nameEn, description,
                        categoryId, unitId, null,
                        purchasePrice, sellingPrice, wholesalePrice,
                        reorderLevel, maxStockLevel,
                        isWeighable, isActive: true, trackExpiry, taxRate,
                        finalImageUrl);

                    if (updateRes.IsFailure)
                    {
                        result.ErrorCount++;
                        result.Errors.Add(new ProductImportErrorDto
                        {
                            RowNumber = rowNum,
                            Barcode = barcode,
                            ProductName = nameAr,
                            ErrorMessage = updateRes.Error.Name
                        });
                        continue;
                    }

                    if (initialStock > 0)
                    {
                        decimal delta = initialStock - existingProduct.QuantityInStock;
                        if (delta != 0)
                        {
                            existingProduct.AdjustStock(delta, allowNegativeStock: true);
                        }
                    }

                    result.SuccessCount++;
                }
                else
                {
                    // Create New Product
                    var productRes = Product.Create(
                        barcode, nameAr, nameEn,
                        categoryId, unitId,
                        purchasePrice, sellingPrice, wholesalePrice,
                        null, description,
                        reorderLevel, maxStockLevel,
                        isWeighable, isActive: true, trackExpiry, taxRate, resolvedImageUrl);

                    if (productRes.IsFailure)
                    {
                        result.ErrorCount++;
                        result.Errors.Add(new ProductImportErrorDto
                        {
                            RowNumber = rowNum,
                            Barcode = barcode,
                            ProductName = nameAr,
                            ErrorMessage = productRes.Error.Name
                        });
                        continue;
                    }

                    var newProd = productRes.Value!;
                    if (initialStock > 0)
                    {
                        newProd.AdjustStock(initialStock, allowNegativeStock: true);
                    }

                    newProductsToAdd.Add(newProd);
                    productMap[barcode.Trim()] = newProd;
                    result.SuccessCount++;
                }
            }

            // Bulk Insert Categories and Units if any missing ones were auto-created
            if (newCategoriesToAdd.Any())
            {
                await _unitOfWork.CategoryRepository.AddRangeAsync(newCategoriesToAdd);
            }

            if (newUnitsToAdd.Any())
            {
                await _unitOfWork.UnitRepository.AddRangeAsync(newUnitsToAdd);
            }

            // Bulk Insert Products using AddRangeAsync
            if (newProductsToAdd.Any())
            {
                await _unitOfWork.ProductRepository.AddRangeAsync(newProductsToAdd);
            }

            // Save all changes in a single high-performance Bulk commit
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<ProductImportResultDto>.Success(result);
        }

        private async Task<string?> ProcessProductImageAsync(
            string barcode,
            string? rawUrl,
            Dictionary<string, ZipArchiveEntry> zipImages,
            CancellationToken ct)
        {
            // 1. First priority: Image in ZIP package matching product Barcode
            if (!string.IsNullOrWhiteSpace(barcode) && zipImages.TryGetValue(barcode.Trim(), out var zipEntry))
            {
                try
                {
                    using var ms = new MemoryStream();
                    using var entryStream = zipEntry.Open();
                    await entryStream.CopyToAsync(ms, ct);
                    var imageBytes = ms.ToArray();

                    if (imageBytes.Length > 0)
                    {
                        var ext = Path.GetExtension(zipEntry.FullName);
                        if (string.IsNullOrWhiteSpace(ext)) ext = ".jpg";

                        var fileName = $"{barcode.Trim()}_{Guid.NewGuid():N}{ext}";
                        ms.Position = 0;
                        IFormFile formFile = new FormFile(ms, 0, imageBytes.Length, "file", fileName)
                        {
                            Headers = new HeaderDictionary(),
                            ContentType = GetContentTypeFromExtension(ext)
                        };

                        var uploadResult = await _fileService.UploadFileAsync(formFile, "uploads/products");
                        if (uploadResult.IsSuccess)
                        {
                            return uploadResult.Value;
                        }
                    }
                }
                catch { }
            }

            // 2. Direct text URL string mode (save directly to DB)
            if (!string.IsNullOrWhiteSpace(rawUrl))
            {
                var trimmed = rawUrl.Trim();
                if (trimmed.StartsWith("www.", StringComparison.OrdinalIgnoreCase))
                {
                    trimmed = "https://" + trimmed;
                }
                else if (trimmed.StartsWith("//", StringComparison.OrdinalIgnoreCase))
                {
                    trimmed = "https:" + trimmed;
                }
                return trimmed;
            }

            return null;
        }

        private static bool IsZipFile(byte[] fileBytes)
        {
            if (fileBytes == null || fileBytes.Length < 4) return false;
            return fileBytes[0] == 0x50 && fileBytes[1] == 0x4B && fileBytes[2] == 0x03 && fileBytes[3] == 0x04;
        }

        private static string GetContentTypeFromExtension(string ext)
        {
            ext = ext.ToLowerInvariant();
            return ext switch
            {
                ".png" => "image/png",
                ".webp" => "image/webp",
                ".gif" => "image/gif",
                _ => "image/jpeg"
            };
        }

        private static bool IsPossibleImageUrl(string val)
        {
            if (string.IsNullOrWhiteSpace(val)) return false;
            var v = val.Trim().ToLowerInvariant();
            return v.StartsWith("http://") ||
                   v.StartsWith("https://") ||
                   v.StartsWith("//") ||
                   v.StartsWith("www.") ||
                   v.EndsWith(".jpg") || v.EndsWith(".jpeg") || v.EndsWith(".png") || v.EndsWith(".webp") || v.EndsWith(".gif") || v.EndsWith(".avif") || v.EndsWith(".svg") ||
                   v.Contains(".jpg?") || v.Contains(".jpeg?") || v.Contains(".png?") || v.Contains(".webp?") ||
                   v.Contains("/images/") || v.Contains("/image/") || v.Contains("/img/");
        }

        private static string GetCellValue(IXLCell cell)
        {
            if (cell == null || cell.IsEmpty()) return string.Empty;

            // 1. Extract URL address if cell contains an Excel Hyperlink object
            try
            {
                if (cell.HasHyperlink)
                {
                    var hyperlink = cell.GetHyperlink();
                    if (hyperlink != null)
                    {
                        var address = hyperlink.ExternalAddress?.ToString()?.Trim();
                        if (!string.IsNullOrWhiteSpace(address))
                        {
                            return address;
                        }

                        var href = hyperlink.InternalAddress?.Trim();
                        if (!string.IsNullOrWhiteSpace(href))
                        {
                            return href;
                        }
                    }
                }
            }
            catch { }

            // 2. Try formatted cell string
            try
            {
                var formatted = cell.GetFormattedString();
                if (!string.IsNullOrWhiteSpace(formatted)) return formatted.Trim();
            }
            catch { }

            // 3. Fallback to raw string / value
            var rawStr = cell.GetValue<string>()?.Trim();
            if (!string.IsNullOrWhiteSpace(rawStr)) return rawStr;

            var valStr = cell.Value.ToString()?.Trim();
            if (!string.IsNullOrWhiteSpace(valStr)) return valStr;

            return string.Empty;
        }

        private static decimal ParseDecimal(string val, decimal defaultValue)
        {
            if (string.IsNullOrWhiteSpace(val)) return defaultValue;
            if (decimal.TryParse(val, out var result)) return result;
            return defaultValue;
        }

        private static bool ParseBool(string val)
        {
            if (string.IsNullOrWhiteSpace(val)) return false;
            val = val.Trim().ToLower();
            return val == "1" || val == "true" || val == "yes" || val == "نعم" || val == "صحيح";
        }
    }
}
