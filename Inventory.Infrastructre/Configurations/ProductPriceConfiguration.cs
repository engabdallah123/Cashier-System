using Inventory.Domain.Pricing.ProductPrices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POS.Shared.Infrastructure.Database;

namespace Inventory.Infrastructre.Configurations;

internal sealed class ProductPriceConfiguration : IEntityTypeConfiguration<ProductPrice>
{
    public void Configure(EntityTypeBuilder<ProductPrice> builder)
    {
        builder.ToTable("ProductPrices", Schemas.Inventory);

        builder.HasKey(pp => pp.Id);

        builder.Property(pp => pp.Price)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(pp => pp.Currency)
            .IsRequired()
            .HasMaxLength(5);

        builder.Property(pp => pp.CreatedAt)
            .IsRequired();

        builder.HasIndex(pp => new { pp.ProductId, pp.PriceListId }).IsUnique();

        builder.HasIndex(pp => pp.ProductId);
        builder.HasIndex(pp => pp.PriceListId);
    }
}
