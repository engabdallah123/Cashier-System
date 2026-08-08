using Inventory.Domain.Pricing.PriceLists;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POS.Shared.Infrastructure.Database;

namespace Inventory.Infrastructre.Configurations;

internal sealed class PriceListConfiguration : IEntityTypeConfiguration<PriceList>
{
    public void Configure(EntityTypeBuilder<PriceList> builder)
    {
        builder.ToTable("PriceLists", Schemas.Inventory);

        builder.HasKey(pl => pl.Id);

        builder.Property(pl => pl.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(pl => pl.Description)
            .HasMaxLength(500);

        builder.Property(pl => pl.IsDefault)
            .IsRequired();

        builder.Property(pl => pl.IsActive)
            .IsRequired();

        builder.Property(pl => pl.CreatedAt)
            .IsRequired();

        builder.HasIndex(pl => pl.Name).IsUnique();
    }
}
