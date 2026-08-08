using Inventory.Application;
using Inventory.Infrastructre;
using POS.Shared.Application;
using POS.Shared.Infrastructure;
using POS.WebAPI.Middlewares;

namespace POS.WebAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add Shared Services
            builder.Services.AddSharedApplication();
            builder.Services.AddSharedInfrastructure(builder.Configuration);

            // Add Inventory Module Services
            builder.Services.AddInventoryApplication();
            builder.Services.AddInventoryInfrastructure(builder.Configuration);

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
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
