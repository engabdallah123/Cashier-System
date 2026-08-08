using MediatR;
using POS.Shared.Domain;
using Microsoft.EntityFrameworkCore;
using POS.Shared.Application.Exceptions;
using POS.Shared.Infrastructure.Database;
using System.Reflection;
using Inventory.Domain.Catalog.Products.Entities;
using Inventory.Domain.Catalog.ProductBarcodes;
using Inventory.Domain.Catalog.Categories;
using Inventory.Domain.Catalog.Brands;
using Inventory.Domain.Catalog.Units;
using Inventory.Domain.Stock.Warehouses;
using Inventory.Domain.Stock.StockBalances;
using Inventory.Domain.Stock.StockMovements;
using Inventory.Domain.Stock.StockTransfers;
using Inventory.Domain.Stock.StockTransferItems;
using Inventory.Domain.Pricing.PriceLists;
using Inventory.Domain.Pricing.ProductPrices;
using Inventory.Domain.Batches.ProductBatches;

namespace Inventory.Infrastructre.Database;

public class InventoryDbContext : DbContext
{
    private readonly IMediator _mediator;

    public InventoryDbContext(DbContextOptions<InventoryDbContext> options, IMediator mediator)
        : base(options)
    {
        _mediator = mediator;
    }

    // Catalog
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductBarcode> ProductBarcodes { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Brand> Brands { get; set; }
    public DbSet<UnitMeasure> Units { get; set; }

    // Stock
    public DbSet<Warehouse> Warehouses { get; set; }
    public DbSet<StockBalance> StockBalances { get; set; }
    public DbSet<StockMovement> StockMovements { get; set; }
    public DbSet<StockTransfer> StockTransfers { get; set; }
    public DbSet<StockTransferItem> StockTransferItems { get; set; }

    // Pricing
    public DbSet<PriceList> PriceLists { get; set; }
    public DbSet<ProductPrice> ProductPrices { get; set; }

    // Batches
    public DbSet<ProductBatch> ProductBatches { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema(Schemas.Inventory);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var domainEvents = ChangeTracker
                .Entries<Entity>()
                .Select(e => e.Entity)
                .Where(e => e.GetDomainEvents().Any())
                .SelectMany(e =>
                {
                    var events = e.GetDomainEvents().ToList();
                    e.ClearDomainEvents();
                    return events;
                })
                .ToList();

            var result = await base.SaveChangesAsync(cancellationToken);

            foreach (var domainEvent in domainEvents)
                await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            var entry = ex.Entries.FirstOrDefault();
            var entityName = entry?.Metadata.ClrType.Name ?? "Unknown Entity";
            var state = entry?.State.ToString() ?? "Unknown State";

            throw new ConcurrencyException(
                $"Concurrency error on Entity: '{entityName}' | State: '{state}'.", ex);
        }
    }
}
