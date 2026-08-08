using Inventory.Domain.Batches.ProductBatches;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POS.Shared.Infrastructure.Database;

namespace Inventory.Infrastructre.Configurations;

internal sealed class ProductBatchConfiguration : IEntityTypeConfiguration<ProductBatch>
{
    public void Configure(EntityTypeBuilder<ProductBatch> builder)
    {
        builder.ToTable("ProductBatches", Schemas.Inventory);

        builder.HasKey(pb => pb.Id);

        builder.Property(pb => pb.BatchNumber)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(pb => pb.Quantity)
            .IsRequired();

        builder.Property(pb => pb.CreatedAt)
            .IsRequired();

        builder.HasIndex(pb => pb.ProductId);
        builder.HasIndex(pb => pb.WarehouseId);
        builder.HasIndex(pb => pb.ExpiryDate);
        builder.HasIndex(pb => new { pb.ProductId, pb.WarehouseId, pb.BatchNumber }).IsUnique();
    }
}
