using Inventory.Domain.Stock.Warehouses;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POS.Shared.Infrastructure.Database;

namespace Inventory.Infrastructre.Configurations;

internal sealed class WarehouseConfiguration : IEntityTypeConfiguration<Warehouse>
{
    public void Configure(EntityTypeBuilder<Warehouse> builder)
    {
        builder.ToTable("Warehouses", Schemas.Inventory);

        builder.HasKey(w => w.Id);

        builder.Property(w => w.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(w => w.Code)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(w => w.Address)
            .HasMaxLength(500);

        builder.Property(w => w.IsActive)
            .IsRequired();

        builder.HasIndex(w => w.Code).IsUnique();
    }
}
