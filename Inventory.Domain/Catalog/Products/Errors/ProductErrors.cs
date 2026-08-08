using POS.Shared.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Domain.Catalog.Products.Errors
{
    public static class ProductErrors
    {
        public static Error NotFound(Guid id) =>
            new("Product.NotFound", $"المنتج بالرقم '{id}' غير موجود.");

        public static Error NotFoundByBarcode(string barcode) =>
            new("Product.NotFoundByBarcode", $"لا يوجد منتج بالباركود '{barcode}'.");

        public static readonly Error InsufficientStock =
            new("Product.InsufficientStock", "الكمية المتاحة غير كافية لإتمام العملية.");

        public static readonly Error DuplicateSku =
            Error.Conflict("Product.DuplicateSku", "يوجد بالفعل منتج بنفس الـ SKU.");

        public static readonly Error DuplicateBarcode =
            Error.Conflict("Product.DuplicateBarcode", "يوجد بالفعل منتج بنفس الباركود.");

        public static readonly Error NameRequired =
            new("Product.NameRequired", "اسم المنتج مطلوب.");

        public static readonly Error LowStockThresholdInvalid =
            new("Product.LowStockThresholdInvalid", "حد المخزون المنخفض لا يمكن أن يكون سالبًا.");
    }
}
