using POS.Shared.Domain;

namespace Inventory.Domain.Catalog.Categories
{
    public sealed class Category : Entity
    {
        public string Name { get; private set; } = default!;
        public string? Description { get; private set; }
        public bool IsActive { get; private set; }
        public DateTime CreatedAt { get; private set; }

        private Category() { } // EF Core

        private Category(Guid id, string name, string? description)
            : base(id)
        {
            Name = name;
            Description = description;
            IsActive = true;
            CreatedAt = DateTime.UtcNow;
        }

        public static Result<Category> Create(string name, string? description = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                return Result<Category>.Failure(CategoryErrors.NameRequired);

            var category = new Category(Guid.NewGuid(), name.Trim(), description?.Trim());
            return Result<Category>.Success(category);
        }

        public Result UpdateInfo(string name, string? description)
        {
            if (string.IsNullOrWhiteSpace(name))
                return Result.Failure(CategoryErrors.NameRequired);

            Name = name.Trim();
            Description = description?.Trim();
            return Result.Success();
        }

        public Result Rename(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return Result.Failure(CategoryErrors.NameRequired);

            Name = name.Trim();
            return Result.Success();
        }

        public Result Activate() { IsActive = true; return Result.Success(); }
        public Result Deactivate() { IsActive = false; return Result.Success(); }
    }
}
