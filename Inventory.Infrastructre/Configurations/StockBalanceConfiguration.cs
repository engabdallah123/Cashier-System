using Inventory.Domain.Stock.StockBalances;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POS.Shared.Infrastructure.Database;

namespace Inventory.Infrastructre.Configurations;

internal sealed class StockBalanceConfiguration : IEntityTypeConfiguration<StockBalance>
{
    public void Configure(EntityTypeBuilder<StockBalance> builder)
    {
        builder.ToTable("StockBalances", Schemas.Inventory);

        builder.HasKey(sb => sb.Id);

        builder.Property(sb => sb.QuantityOnHand)
            .IsRequired();

        builder.Property(sb => sb.LastUpdated)
            .IsRequired();

        builder.HasIndex(sb => new { sb.ProductId, sb.WarehouseId }).IsUnique();

        builder.HasIndex(sb => sb.ProductId);
        builder.HasIndex(sb => sb.WarehouseId);
    }
}
