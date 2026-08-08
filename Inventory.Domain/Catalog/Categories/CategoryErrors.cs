using POS.Shared.Domain;

namespace Inventory.Domain.Catalog.Categories
{
    public static class CategoryErrors
    {
        public static Error NotFound(Guid id) =>
            new("Category.NotFound", $"التصنيف بالرقم '{id}' غير موجود.");

        public static readonly Error NameRequired =
            new("Category.NameRequired", "اسم التصنيف مطلوب.");

        public static readonly Error DuplicateName =
            Error.Conflict("Category.DuplicateName", "يوجد بالفعل تصنيف بنفس الاسم.");
    }
}
