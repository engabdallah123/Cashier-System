using POS.Shared.Domain;

namespace Inventory.Domain.Catalog.Units
{
    public static class UnitErrors
    {
        public static Error NotFound(Guid id) =>
            new("Unit.NotFound", $"الوحدة بالرقم '{id}' غير موجودة.");

        public static readonly Error NameRequired =
            new("Unit.NameRequired", "اسم الوحدة مطلوب.");

        public static readonly Error AbbreviationRequired =
            new("Unit.AbbreviationRequired", "اختصار الوحدة مطلوب.");

        public static readonly Error DuplicateName =
            Error.Conflict("Unit.DuplicateName", "يوجد بالفعل وحدة بنفس الاسم.");
    }
}
