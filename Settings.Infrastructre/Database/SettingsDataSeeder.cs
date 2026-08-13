using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Settings.Domain.StoreSettings.Entities;

namespace Settings.Infrastructre.Database;

public static class SettingsDataSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SettingsDbContext>();

        await context.Database.MigrateAsync();

        if (!await context.StoreSettings.AnyAsync())
        {
            var defaultSetting = StoreSetting.Create(
                storeName: "المتجر الرئيسي",
                currency: "EGP",
                taxRate: 0,
                isTaxIncluded: true,
                address: "القاهرة، مصر",
                phone: null,
                invoiceFooterMessage: "شكراً لزيارتكم!",
                allowNegativeStock: false);

            if (defaultSetting.IsSuccess)
            {
                await context.StoreSettings.AddAsync(defaultSetting.Value);
                await context.SaveChangesAsync();
            }
        }
    }
}
