using Inventory.Domain.Stock.StockTransfers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POS.Shared.Infrastructure.Database;

namespace Inventory.Infrastructre.Configurations;

internal sealed class StockTransferConfiguration : IEntityTypeConfiguration<StockTransfer>
{
    public void Configure(EntityTypeBuilder<StockTransfer> builder)
    {
        builder.ToTable("StockTransfers", Schemas.Inventory);

        builder.HasKey(t => t.Id);

        builder.Property(t => t.TransferNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(t => t.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(t => t.Notes)
            .HasMaxLength(500);

        builder.Property(t => t.CreatedBy)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.CreatedAt)
            .IsRequired();

        builder.HasMany(t => t.Items)
            .WithOne()
            .HasForeignKey(i => i.StockTransferId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(t => t.TransferNumber).IsUnique();
        builder.HasIndex(t => t.SourceWarehouseId);
        builder.HasIndex(t => t.DestinationWarehouseId);
        builder.HasIndex(t => t.Status);
    }
}
