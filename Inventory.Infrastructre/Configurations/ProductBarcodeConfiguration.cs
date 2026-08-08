using Inventory.Domain.Catalog.ProductBarcodes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POS.Shared.Infrastructure.Database;

namespace Inventory.Infrastructre.Configurations;

internal sealed class ProductBarcodeConfiguration : IEntityTypeConfiguration<ProductBarcode>
{
    public void Configure(EntityTypeBuilder<ProductBarcode> builder)
    {
        builder.ToTable("ProductBarcodes", Schemas.Inventory);

        builder.HasKey(pb => pb.Id);

        builder.Property(pb => pb.Barcode)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(pb => pb.IsDefault)
            .IsRequired();

        builder.Property(pb => pb.CreatedAt)
            .IsRequired();

        builder.HasIndex(pb => pb.Barcode).IsUnique();
        builder.HasIndex(pb => pb.ProductId);
    }
}
