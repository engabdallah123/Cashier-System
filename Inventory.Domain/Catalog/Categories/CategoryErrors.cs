using POS.Shared.Domain;

namespace Inventory.Domain.Catalog.Categories
{
    public static class CategoryErrors
    {
        public static Error NotFound(Guid id) =>
            new("Category.NotFound", $"التصنيف بالرقم '{id}' غير موجود.");

        public static readonly Error NameArRequired =
            new("Category.NameArRequired", "اسم التصنيف بالعربية مطلوب.");

        public static readonly Error NameEnRequired =
            new("Category.NameEnRequired", "اسم التصنيف بالإنجليزية مطلوب.");

        public static readonly Error DuplicateName =
            Error.Conflict("Category.DuplicateName", "يوجد بالفعل تصنيف بنفس الاسم.");
    }
}
