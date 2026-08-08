using POS.Shared.Domain;

namespace Inventory.Domain.Catalog.Units
{
    public sealed class UnitMeasure : Entity
    {
        public string Name { get; private set; } = default!;
        public string Abbreviation { get; private set; } = default!;
        public bool IsActive { get; private set; }
        public DateTime CreatedAt { get; private set; }

        private UnitMeasure() { } // EF Core

        private UnitMeasure(Guid id, string name, string abbreviation)
            : base(id)
        {
            Name = name;
            Abbreviation = abbreviation;
            IsActive = true;
            CreatedAt = DateTime.UtcNow;
        }

        public static Result<UnitMeasure> Create(string name, string abbreviation)
        {
            if (string.IsNullOrWhiteSpace(name))
                return Result<UnitMeasure>.Failure(UnitErrors.NameRequired);

            if (string.IsNullOrWhiteSpace(abbreviation))
                return Result<UnitMeasure>.Failure(UnitErrors.AbbreviationRequired);

            var unit = new UnitMeasure(Guid.NewGuid(), name.Trim(), abbreviation.Trim());
            return Result<UnitMeasure>.Success(unit);
        }

        public Result UpdateInfo(string name, string abbreviation)
        {
            if (string.IsNullOrWhiteSpace(name))
                return Result.Failure(UnitErrors.NameRequired);

            if (string.IsNullOrWhiteSpace(abbreviation))
                return Result.Failure(UnitErrors.AbbreviationRequired);

            Name = name.Trim();
            Abbreviation = abbreviation.Trim();
            return Result.Success();
        }

        public Result Activate() { IsActive = true; return Result.Success(); }
        public Result Deactivate() { IsActive = false; return Result.Success(); }
    }
}
