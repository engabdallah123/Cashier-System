using POS.Shared.Domain;

namespace Inventory.Domain.Catalog.Categories
{
    public sealed class Category : Entity
    {
        public string NameAr { get; private set; } = default!;
        public string NameEn { get; private set; } = default!;
        public Guid? ParentCategoryId { get; private set; }
        public bool IsActive { get; private set; }
        public DateTime CreatedAt { get; private set; }

        private Category() { } // EF Core

        private Category(Guid id, string nameAr, string nameEn, Guid? parentCategoryId)
            : base(id)
        {
            NameAr = nameAr;
            NameEn = nameEn;
            ParentCategoryId = parentCategoryId;
            IsActive = true;
            CreatedAt = DateTime.UtcNow;
        }

        public static Result<Category> Create(string nameAr, string nameEn, Guid? parentCategoryId = null)
        {
            if (string.IsNullOrWhiteSpace(nameAr))
                return Result<Category>.Failure(CategoryErrors.NameArRequired);

            if (string.IsNullOrWhiteSpace(nameEn))
                return Result<Category>.Failure(CategoryErrors.NameEnRequired);

            var category = new Category(Guid.NewGuid(), nameAr.Trim(), nameEn.Trim(), parentCategoryId);
            return Result<Category>.Success(category);
        }

        public Result Update(string nameAr, string nameEn, Guid? parentCategoryId)
        {
            if (string.IsNullOrWhiteSpace(nameAr))
                return Result.Failure(CategoryErrors.NameArRequired);

            if (string.IsNullOrWhiteSpace(nameEn))
                return Result.Failure(CategoryErrors.NameEnRequired);

            NameAr = nameAr.Trim();
            NameEn = nameEn.Trim();
            ParentCategoryId = parentCategoryId;
            return Result.Success();
        }

        public Result Activate() { IsActive = true; return Result.Success(); }
        public Result Deactivate() { IsActive = false; return Result.Success(); }
    }
}
