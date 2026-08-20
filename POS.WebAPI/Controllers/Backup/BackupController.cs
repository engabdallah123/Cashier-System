using Dapper;
using Microsoft.AspNetCore.Mvc;
using POS.Shared.Application.Database;
using System.Text.Json;

namespace POS.WebAPI.Controllers.Backup
{
    [ApiController]
    [Route("api/backup")]
    public class BackupController : ControllerBase
    {
        private readonly ISqlConnectionFactory _sqlConnectionFactory;

        public BackupController(ISqlConnectionFactory sqlConnectionFactory)
        {
            _sqlConnectionFactory = sqlConnectionFactory;
        }

        [HttpGet("export")]
        public async Task<IActionResult> Export(CancellationToken ct)
        {
            using var connection = _sqlConnectionFactory.CreateConnection();

            var backup = new
            {
                ExportedAt = DateTime.UtcNow,
                Version = "1.0",
                Products = (await connection.QueryAsync("SELECT * FROM [Inventory].[Products]")).AsList(),
                Categories = (await connection.QueryAsync("SELECT * FROM [Inventory].[Categories]")).AsList(),
                Units = (await connection.QueryAsync("SELECT * FROM [Inventory].[Units]")).AsList(),
                Customers = (await connection.QueryAsync("SELECT * FROM [Sales].[Customers]")).AsList(),
                Suppliers = (await connection.QueryAsync("SELECT * FROM [Purchases].[Suppliers]")).AsList(),
                Sales = (await connection.QueryAsync("SELECT * FROM [Sales].[Sales]")).AsList(),
                SaleItems = (await connection.QueryAsync("SELECT * FROM [Sales].[SaleItems]")).AsList(),
                Purchases = (await connection.QueryAsync("SELECT * FROM [Purchases].[Purchases]")).AsList(),
                PurchaseItems = (await connection.QueryAsync("SELECT * FROM [Purchases].[PurchaseItems]")).AsList(),
                SalesReturns = (await connection.QueryAsync("SELECT * FROM [Returns].[SalesReturns]")).AsList(),
                SalesReturnItems = (await connection.QueryAsync("SELECT * FROM [Returns].[SalesReturnItems]")).AsList(),
                PurchaseReturns = (await connection.QueryAsync("SELECT * FROM [Returns].[PurchaseReturns]")).AsList(),
                PurchaseReturnItems = (await connection.QueryAsync("SELECT * FROM [Returns].[PurchaseReturnItems]")).AsList(),
                Expenses = (await connection.QueryAsync("SELECT * FROM [Expenses].[Expenses]")).AsList(),
                Shifts = (await connection.QueryAsync("SELECT * FROM [Shifts].[Shifts]")).AsList(),
                StoreSettings = (await connection.QueryAsync("SELECT * FROM [Settings].[StoreSettings]")).AsList()
            };

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            var json = JsonSerializer.Serialize(backup, options);
            var bytes = System.Text.Encoding.UTF8.GetBytes(json);
            var fileName = $"POS_Backup_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.json";

            return File(bytes, "application/json", fileName);
        }
    }
}
