using Identity.Domain.Users.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Identity.Infrastructre.Database;

public static class IdentityDataSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IdentityModuleDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        // Ensure database migrations are applied
        await context.Database.MigrateAsync();

        // 1. Seed Roles
        string[] roles = ["Admin", "Cashier", "Manager"];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // 2. Seed Default Admin User
        var adminUser = await userManager.FindByNameAsync("admin");
        if (adminUser is null)
        {
            adminUser = new ApplicationUser
            { 
                UserName = "admin",
                Email = "admin@pos.local",
                FullName = "System Administrator",
                PhoneNumber = "+201000000000",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(adminUser, "Admin123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }

        // 3. Seed Default Cashier User
        var cashierUser = await userManager.FindByNameAsync("cashier");
        if (cashierUser is null)
        {
            cashierUser = new ApplicationUser
            {
                UserName = "cashier",
                Email = "cashier@pos.local",
                FullName = "Default Cashier",
                PhoneNumber = "+201000000000",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(cashierUser, "Cashier123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(cashierUser, "Cashier");
            }
        }

        // 4. seed Default Manager User
        var managerUser = await userManager.FindByNameAsync("manager");
        if (managerUser is null)
        {
            managerUser = new ApplicationUser
            {
                UserName = "manager",
                Email = "manager@pos.local",
                FullName = "Default Manager",
                PhoneNumber = "+201000000001",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(managerUser, "Manager123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(managerUser, "Manager");
            }
        }
    }
}
