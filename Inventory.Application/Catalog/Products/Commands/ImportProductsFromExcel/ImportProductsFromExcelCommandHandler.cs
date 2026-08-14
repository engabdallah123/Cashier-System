using ClosedXML.Excel;
using Inventory.Domain;
using Inventory.Domain.Catalog.Categories;
using Inventory.Domain.Catalog.Products.Entities;
using Inventory.Domain.Catalog.Units;
using POS.Shared.Application.Messaging;
using POS.Shared.Domain;

namespace Inventory.Application.Catalog.Products.Commands.ImportProductsFromExcel
{
    internal sealed class ImportProductsFromExcelCommandHandler
        : ICommandHandler<ImportProductsFromExcelCommand, ProductImportResultDto>
    {
        private readonly IInventoryUnitOfWork _unitOfWork;

        public ImportProductsFromExcelCommandHandler(IInventoryUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
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

            // Load existing categories and units for lookup and comparison
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

            using var stream = new MemoryStream(request.FileBytes);
            using var workbook = new XLWorkbook(stream);
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

            int lastRow = range.LastRow().RowNumber();
            result.TotalRows = lastRow - 1; // Subtract 1 for header row

            // Track barcodes seen within this file batch
            var batchBarcodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (int rowNum = 2; rowNum <= lastRow; rowNum++)
            {
                var row = worksheet.Row(rowNum);

                // Read cell values
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
                string description = GetCellValue(row.Cell(15));

                // Basic Row Validations
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

                // Check duplicate barcode inside same Excel batch
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

                // Fill English name if omitted
                if (string.IsNullOrWhiteSpace(nameEn))
                {
                    nameEn = nameAr;
                }

                // 1. Resolve Category (Match existing or Add new)
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
                    await _unitOfWork.CategoryRepository.AddAsync(newCat);
                    categoryMap[catKey] = newCat;
                    categoryId = newCat.Id;
                }

                // 2. Resolve Unit (Match existing or Add new)
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
                    await _unitOfWork.UnitRepository.AddAsync(newUnit);
                    unitMap[uKey] = newUnit;
                    unitId = newUnit.Id;
                }

                // 3. Resolve Product in Database
                var existingProduct = await _unitOfWork.ProductRepository.GetByBarcodeAsync(barcode, cancellationToken);

                if (existingProduct != null)
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

                    // Update Existing Product
                    var updateRes = existingProduct.Update(
                        barcode, nameAr, nameEn, description,
                        categoryId, unitId, null,
                        purchasePrice, sellingPrice, wholesalePrice,
                        reorderLevel, maxStockLevel,
                        isWeighable, isActive: true, trackExpiry, taxRate,
                        existingProduct.ImageUrl);

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
                        isWeighable, isActive: true, trackExpiry, taxRate, null);

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

                    await _unitOfWork.ProductRepository.AddAsync(newProd);
                    result.SuccessCount++;
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<ProductImportResultDto>.Success(result);
        }

        private static string GetCellValue(IXLCell cell)
        {
            if (cell == null || cell.IsEmpty()) return string.Empty;
            return cell.GetValue<string>()?.Trim() ?? string.Empty;
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
