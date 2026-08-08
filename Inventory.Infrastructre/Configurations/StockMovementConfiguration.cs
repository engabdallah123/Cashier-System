using Inventory.Domain.Stock.StockMovements;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POS.Shared.Infrastructure.Database;

namespace Inventory.Infrastructre.Configurations;

internal sealed class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        builder.ToTable("StockMovements", Schemas.Inventory);

        builder.HasKey(sm => sm.Id);

        builder.Property(sm => sm.Type)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(sm => sm.Quantity)
            .IsRequired();

        builder.Property(sm => sm.BeforeQuantity)
            .IsRequired();

        builder.Property(sm => sm.AfterQuantity)
            .IsRequired();

        builder.Property(sm => sm.ReferenceType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(sm => sm.CreatedBy)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(sm => sm.CreatedAt)
            .IsRequired();

        builder.HasIndex(sm => sm.ProductId);
        builder.HasIndex(sm => sm.WarehouseId);
        builder.HasIndex(sm => sm.CreatedAt);
        builder.HasIndex(sm => sm.Type);
    }
}
