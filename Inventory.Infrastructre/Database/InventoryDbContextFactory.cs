using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Inventory.Infrastructre.Database
{
    public class InventoryDbContextFactory : IDesignTimeDbContextFactory<InventoryDbContext>
    {
        public InventoryDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<InventoryDbContext>();
            
            // Design-time fallback connection string for generating migrations
            const string connectionString = "Data Source=.;Initial Catalog=Cashier;Integrated Security=True;Trust Server Certificate=True";
            
            optionsBuilder.UseSqlServer(connectionString);

            // Pass null for IMediator during design-time migration generation
            return new InventoryDbContext(optionsBuilder.Options, null!);
        }
    }
}
