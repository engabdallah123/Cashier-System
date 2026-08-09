using Audit.Application;
using Audit.Infrastructre;
using Dashboard.Application;
using Expenses.Application;
using Expenses.Infrastructre;
using Identity.Application;
using Identity.Infrastructre;
using Inventory.Application;
using Inventory.Infrastructre;
using POS.Shared.Application;
using POS.Shared.Infrastructure;
using POS.WebAPI.Middlewares;
using Purchases.Application;
using Purchases.Infrastructre;
using Returns.Application;
using Returns.Infrastructre;
using Sales.Application;
using Sales.Infrastructre;
using Settings.Application;
using Settings.Infrastructre;
using Shifts.Application;
using Shifts.Infrastructre;

namespace POS.WebAPI
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            // Set QuestPDF License to Community
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

            var builder = WebApplication.CreateBuilder(args);

            // Add Shared Services
            builder.Services.AddSharedApplication();
            builder.Services.AddSharedInfrastructure(builder.Configuration);

            // Add Identity Services
            builder.Services.AddIdentityApplication();
            builder.Services.AddIdentityInfrastructure(builder.Configuration);

            // Add Shifts Services
            builder.Services.AddShiftsApplication();
            builder.Services.AddShiftsInfrastructure(builder.Configuration);

            // Add Inventory Module Services
            builder.Services.AddInventoryApplication();
            builder.Services.AddInventoryInfrastructure(builder.Configuration);

            // Add Purchases Module Services
            builder.Services.AddPurchasesApplication();
            builder.Services.AddPurchasesInfrastructure(builder.Configuration);

            // Add Sales Module Services
            builder.Services.AddSalesApplication();
            builder.Services.AddSalesInfrastructure(builder.Configuration);

            // Add Returns Module Services
            builder.Services.AddReturnsApplication();
            builder.Services.AddReturnsInfrastructure(builder.Configuration);

            // Add Expenses Module Services
            builder.Services.AddExpensesApplication();
            builder.Services.AddExpensesInfrastructure(builder.Configuration);

            // Add Dashboard Module Services
            builder.Services.AddDashboardApplication();

            // Add Settings Module Services
            builder.Services.AddSettingsApplication();
            builder.Services.AddSettingsInfrastructure(builder.Configuration);

            // Add Audit Module Services
            builder.Services.AddAuditApplication();
            builder.Services.AddAuditInfrastructure(builder.Configuration);

            builder.Services.AddControllers();

            // Swagger / OpenAPI Configuration
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                    builder =>
                    {
                        builder.AllowAnyOrigin()
                            .AllowAnyMethod()
                            .AllowAnyHeader();
                    });
            });

            var app = builder.Build();

            // Seed Identity Data (Roles & Default Accounts: admin/Admin123!, cashier/Cashier123!)
            await IdentityDataSeeder.SeedAsync(app.Services);

            // Enable Swagger UI
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "POS Cashier System API v1");
                c.RoutePrefix = "swagger";
            });

            app.UseHttpsRedirection();
            app.UseCors("AllowAll");
            app.UseCustomExceptionHandler();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
