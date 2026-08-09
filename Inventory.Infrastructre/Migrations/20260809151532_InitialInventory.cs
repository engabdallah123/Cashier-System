using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory.Infrastructre.Migrations
{
    /// <inheritdoc />
    public partial class InitialInventory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Brands",
                schema: "Inventory");

            migrationBuilder.DropTable(
                name: "PriceLists",
                schema: "Inventory");

            migrationBuilder.DropTable(
                name: "ProductBarcodes",
                schema: "Inventory");

            migrationBuilder.DropTable(
                name: "ProductBatches",
                schema: "Inventory");

            migrationBuilder.DropTable(
                name: "ProductPrices",
                schema: "Inventory");

            migrationBuilder.DropTable(
                name: "StockBalances",
                schema: "Inventory");

            migrationBuilder.DropTable(
                name: "StockTransferItems",
                schema: "Inventory");

            migrationBuilder.DropTable(
                name: "Warehouses",
                schema: "Inventory");

            migrationBuilder.DropTable(
                name: "StockTransfers",
                schema: "Inventory");

            migrationBuilder.DropIndex(
                name: "IX_Units_Name",
                schema: "Inventory",
                table: "Units");

            migrationBuilder.DropIndex(
                name: "IX_StockMovements_WarehouseId",
                schema: "Inventory",
                table: "StockMovements");

            migrationBuilder.DropIndex(
                name: "IX_Categories_Name",
                schema: "Inventory",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                schema: "Inventory",
                table: "Units");

            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "Inventory",
                table: "Units");

            migrationBuilder.DropColumn(
                name: "AfterQuantity",
                schema: "Inventory",
                table: "StockMovements");

            migrationBuilder.DropColumn(
                name: "BeforeQuantity",
                schema: "Inventory",
                table: "StockMovements");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "Inventory",
                table: "StockMovements");

            migrationBuilder.DropColumn(
                name: "ReferenceId",
                schema: "Inventory",
                table: "StockMovements");

            migrationBuilder.DropColumn(
                name: "ReferenceType",
                schema: "Inventory",
                table: "StockMovements");

            migrationBuilder.DropColumn(
                name: "Currency",
                schema: "Inventory",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "LowStockThreshold",
                schema: "Inventory",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "QuantityOnHand",
                schema: "Inventory",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Description",
                schema: "Inventory",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "Name",
                schema: "Inventory",
                table: "Categories");

            migrationBuilder.RenameColumn(
                name: "Name",
                schema: "Inventory",
                table: "Units",
                newName: "NameEn");

            migrationBuilder.RenameColumn(
                name: "Abbreviation",
                schema: "Inventory",
                table: "Units",
                newName: "Symbol");

            migrationBuilder.RenameColumn(
                name: "WarehouseId",
                schema: "Inventory",
                table: "StockMovements",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                schema: "Inventory",
                table: "StockMovements",
                newName: "MovementDate");

            migrationBuilder.RenameIndex(
                name: "IX_StockMovements_CreatedAt",
                schema: "Inventory",
                table: "StockMovements",
                newName: "IX_StockMovements_MovementDate");

            migrationBuilder.RenameColumn(
                name: "Sku",
                schema: "Inventory",
                table: "Products",
                newName: "Barcode");

            migrationBuilder.RenameColumn(
                name: "Price",
                schema: "Inventory",
                table: "Products",
                newName: "WholesalePrice");

            migrationBuilder.RenameColumn(
                name: "Name",
                schema: "Inventory",
                table: "Products",
                newName: "NameEn");

            migrationBuilder.RenameColumn(
                name: "BrandId",
                schema: "Inventory",
                table: "Products",
                newName: "SupplierId");

            migrationBuilder.RenameIndex(
                name: "IX_Products_Sku",
                schema: "Inventory",
                table: "Products",
                newName: "IX_Products_Barcode");

            migrationBuilder.RenameIndex(
                name: "IX_Products_BrandId",
                schema: "Inventory",
                table: "Products",
                newName: "IX_Products_SupplierId");

            migrationBuilder.AddColumn<string>(
                name: "NameAr",
                schema: "Inventory",
                table: "Units",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<decimal>(
                name: "Quantity",
                schema: "Inventory",
                table: "StockMovements",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                schema: "Inventory",
                table: "StockMovements",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Reference",
                schema: "Inventory",
                table: "StockMovements",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "UnitId",
                schema: "Inventory",
                table: "Products",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "CategoryId",
                schema: "Inventory",
                table: "Products",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                schema: "Inventory",
                table: "Products",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                schema: "Inventory",
                table: "Products",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsWeighable",
                schema: "Inventory",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "MaxStockLevel",
                schema: "Inventory",
                table: "Products",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "NameAr",
                schema: "Inventory",
                table: "Products",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "PurchasePrice",
                schema: "Inventory",
                table: "Products",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "QuantityInStock",
                schema: "Inventory",
                table: "Products",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ReorderLevel",
                schema: "Inventory",
                table: "Products",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "SellingPrice",
                schema: "Inventory",
                table: "Products",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TaxRate",
                schema: "Inventory",
                table: "Products",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "TrackExpiry",
                schema: "Inventory",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "NameAr",
                schema: "Inventory",
                table: "Categories",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NameEn",
                schema: "Inventory",
                table: "Categories",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "ParentCategoryId",
                schema: "Inventory",
                table: "Categories",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categories_ParentCategoryId",
                schema: "Inventory",
                table: "Categories",
                column: "ParentCategoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Categories_ParentCategoryId",
                schema: "Inventory",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "NameAr",
                schema: "Inventory",
                table: "Units");

            migrationBuilder.DropColumn(
                name: "Notes",
                schema: "Inventory",
                table: "StockMovements");

            migrationBuilder.DropColumn(
                name: "Reference",
                schema: "Inventory",
                table: "StockMovements");

            migrationBuilder.DropColumn(
                name: "Description",
                schema: "Inventory",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                schema: "Inventory",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "IsWeighable",
                schema: "Inventory",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "MaxStockLevel",
                schema: "Inventory",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "NameAr",
                schema: "Inventory",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "PurchasePrice",
                schema: "Inventory",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "QuantityInStock",
                schema: "Inventory",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ReorderLevel",
                schema: "Inventory",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "SellingPrice",
                schema: "Inventory",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "TaxRate",
                schema: "Inventory",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "TrackExpiry",
                schema: "Inventory",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "NameAr",
                schema: "Inventory",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "NameEn",
                schema: "Inventory",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "ParentCategoryId",
                schema: "Inventory",
                table: "Categories");

            migrationBuilder.RenameColumn(
                name: "Symbol",
                schema: "Inventory",
                table: "Units",
                newName: "Abbreviation");

            migrationBuilder.RenameColumn(
                name: "NameEn",
                schema: "Inventory",
                table: "Units",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "UserId",
                schema: "Inventory",
                table: "StockMovements",
                newName: "WarehouseId");

            migrationBuilder.RenameColumn(
                name: "MovementDate",
                schema: "Inventory",
                table: "StockMovements",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_StockMovements_MovementDate",
                schema: "Inventory",
                table: "StockMovements",
                newName: "IX_StockMovements_CreatedAt");

            migrationBuilder.RenameColumn(
                name: "WholesalePrice",
                schema: "Inventory",
                table: "Products",
                newName: "Price");

            migrationBuilder.RenameColumn(
                name: "SupplierId",
                schema: "Inventory",
                table: "Products",
                newName: "BrandId");

            migrationBuilder.RenameColumn(
                name: "NameEn",
                schema: "Inventory",
                table: "Products",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "Barcode",
                schema: "Inventory",
                table: "Products",
                newName: "Sku");

            migrationBuilder.RenameIndex(
                name: "IX_Products_SupplierId",
                schema: "Inventory",
                table: "Products",
                newName: "IX_Products_BrandId");

            migrationBuilder.RenameIndex(
                name: "IX_Products_Barcode",
                schema: "Inventory",
                table: "Products",
                newName: "IX_Products_Sku");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                schema: "Inventory",
                table: "Units",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "Inventory",
                table: "Units",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<int>(
                name: "Quantity",
                schema: "Inventory",
                table: "StockMovements",
                type: "int",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,3)",
                oldPrecision: 18,
                oldScale: 3);

            migrationBuilder.AddColumn<int>(
                name: "AfterQuantity",
                schema: "Inventory",
                table: "StockMovements",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "BeforeQuantity",
                schema: "Inventory",
                table: "StockMovements",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                schema: "Inventory",
                table: "StockMovements",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "ReferenceId",
                schema: "Inventory",
                table: "StockMovements",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReferenceType",
                schema: "Inventory",
                table: "StockMovements",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<Guid>(
                name: "UnitId",
                schema: "Inventory",
                table: "Products",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "CategoryId",
                schema: "Inventory",
                table: "Products",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                schema: "Inventory",
                table: "Products",
                type: "nvarchar(5)",
                maxLength: 5,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "LowStockThreshold",
                schema: "Inventory",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "QuantityOnHand",
                schema: "Inventory",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                schema: "Inventory",
                table: "Categories",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                schema: "Inventory",
                table: "Categories",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Brands",
                schema: "Inventory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Brands", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PriceLists",
                schema: "Inventory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriceLists", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductBarcodes",
                schema: "Inventory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Barcode = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductBarcodes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductBatches",
                schema: "Inventory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BatchNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    WarehouseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductBatches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductPrices",
                schema: "Inventory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PriceListId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductPrices", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StockBalances",
                schema: "Inventory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuantityOnHand = table.Column<int>(type: "int", nullable: false),
                    WarehouseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockBalances", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StockTransfers",
                schema: "Inventory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DestinationWarehouseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExecutedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SourceWarehouseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    TransferNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockTransfers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Warehouses",
                schema: "Inventory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Warehouses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StockTransferItems",
                schema: "Inventory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    StockTransferId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockTransferItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockTransferItems_StockTransfers_StockTransferId",
                        column: x => x.StockTransferId,
                        principalSchema: "Inventory",
                        principalTable: "StockTransfers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Units_Name",
                schema: "Inventory",
                table: "Units",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_WarehouseId",
                schema: "Inventory",
                table: "StockMovements",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Name",
                schema: "Inventory",
                table: "Categories",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Brands_Name",
                schema: "Inventory",
                table: "Brands",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PriceLists_Name",
                schema: "Inventory",
                table: "PriceLists",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductBarcodes_Barcode",
                schema: "Inventory",
                table: "ProductBarcodes",
                column: "Barcode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductBarcodes_ProductId",
                schema: "Inventory",
                table: "ProductBarcodes",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductBatches_ExpiryDate",
                schema: "Inventory",
                table: "ProductBatches",
                column: "ExpiryDate");

            migrationBuilder.CreateIndex(
                name: "IX_ProductBatches_ProductId",
                schema: "Inventory",
                table: "ProductBatches",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductBatches_ProductId_WarehouseId_BatchNumber",
                schema: "Inventory",
                table: "ProductBatches",
                columns: new[] { "ProductId", "WarehouseId", "BatchNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductBatches_WarehouseId",
                schema: "Inventory",
                table: "ProductBatches",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductPrices_PriceListId",
                schema: "Inventory",
                table: "ProductPrices",
                column: "PriceListId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductPrices_ProductId",
                schema: "Inventory",
                table: "ProductPrices",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductPrices_ProductId_PriceListId",
                schema: "Inventory",
                table: "ProductPrices",
                columns: new[] { "ProductId", "PriceListId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StockBalances_ProductId",
                schema: "Inventory",
                table: "StockBalances",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_StockBalances_ProductId_WarehouseId",
                schema: "Inventory",
                table: "StockBalances",
                columns: new[] { "ProductId", "WarehouseId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StockBalances_WarehouseId",
                schema: "Inventory",
                table: "StockBalances",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_StockTransferItems_ProductId",
                schema: "Inventory",
                table: "StockTransferItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_StockTransferItems_StockTransferId",
                schema: "Inventory",
                table: "StockTransferItems",
                column: "StockTransferId");

            migrationBuilder.CreateIndex(
                name: "IX_StockTransfers_DestinationWarehouseId",
                schema: "Inventory",
                table: "StockTransfers",
                column: "DestinationWarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_StockTransfers_SourceWarehouseId",
                schema: "Inventory",
                table: "StockTransfers",
                column: "SourceWarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_StockTransfers_Status",
                schema: "Inventory",
                table: "StockTransfers",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_StockTransfers_TransferNumber",
                schema: "Inventory",
                table: "StockTransfers",
                column: "TransferNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Warehouses_Code",
                schema: "Inventory",
                table: "Warehouses",
                column: "Code",
                unique: true);
        }
    }
}
