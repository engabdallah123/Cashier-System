using Inventory.Application.Catalog.Products.Commands.CreateProduct;
using Inventory.Application.Catalog.Products.Commands.DeleteProduct;
using Inventory.Application.Catalog.Products.Commands.ImportProductsFromExcel;
using Inventory.Application.Catalog.Products.Commands.UpdateProduct;
using Inventory.Application.Catalog.Products.Queries.GetProductByBarcode;
using Inventory.Application.Catalog.Products.Queries.GetProductById;
using Inventory.Application.Catalog.Products.Queries.GetProductExcelTemplate;
using Inventory.Application.Catalog.Products.Queries.GetProducts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Shared.Application.IService;

namespace POS.WebAPI.Controllers.Inventory
{
    public class CreateProductRequest
    {
        public string Barcode { get; set; } = default!;
        public string NameAr { get; set; } = default!;
        public string NameEn { get; set; } = default!;
        public string? Description { get; set; }
        public Guid CategoryId { get; set; }
        public Guid UnitId { get; set; }
        public Guid? SupplierId { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal SellingPrice { get; set; }
        public decimal WholesalePrice { get; set; }
        public decimal InitialStock { get; set; } = 0;
        public decimal ReorderLevel { get; set; } = 5;
        public decimal MaxStockLevel { get; set; } = 100;
        public bool IsWeighable { get; set; }
        public bool IsActive { get; set; } = true;
        public bool TrackExpiry { get; set; }
        public decimal TaxRate { get; set; }
        public IFormFile? ImageFile { get; set; }
    }

    [ApiController]
    [Route("api/inventory/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IMediator _sender;
        private readonly IFileService _fileService;

        public ProductsController(IMediator sender, IFileService fileService)
        {
            _sender = sender;
            _fileService = fileService;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Create([FromForm] CreateProductRequest request, CancellationToken ct)
        {
            string? imageUrl = null;
            if (request.ImageFile != null && request.ImageFile.Length > 0)
            {
                var uploadResult = await _fileService.UploadFileAsync(request.ImageFile, "uploads/products");
                if (uploadResult.IsFailure)
                    return BadRequest(uploadResult.Error);
                imageUrl = uploadResult.Value;
            }

            var command = new CreateProductCommand(
                request.Barcode, request.NameAr, request.NameEn,
                request.CategoryId, request.UnitId,
                request.PurchasePrice, request.SellingPrice, request.WholesalePrice,
                request.SupplierId, request.Description,
                request.ReorderLevel, request.MaxStockLevel,
                request.IsWeighable, request.IsActive, request.TrackExpiry,
                request.TaxRate, imageUrl, request.InitialStock);

            var result = await _sender.Send(command, ct);
            if (result.IsFailure)
                return BadRequest(result.Error);

            return CreatedAtAction(nameof(GetById), new { id = result.Value }, result.Value);
        }

        [HttpPost("json")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateJson([FromBody] CreateProductCommand command, CancellationToken ct)
        {
            var result = await _sender.Send(command, ct);
            if (result.IsFailure)
                return BadRequest(result.Error);

            return CreatedAtAction(nameof(GetById), new { id = result.Value }, result.Value);
        }

        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductCommand command, CancellationToken ct)
        {
            if (id != command.Id)
                return BadRequest("Product ID mismatch.");

            var result = await _sender.Send(command, ct);
            if (result.IsFailure)
                return BadRequest(result.Error);

            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            var result = await _sender.Send(new DeleteProductCommand(id), ct);
            if (result.IsFailure)
                return BadRequest(result.Error);

            return NoContent();
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        {
            var result = await _sender.Send(new GetProductByIdQuery(id), ct);
            if (result.IsFailure)
                return NotFound(result.Error);

            return Ok(result.Value);
        }

        [HttpGet("barcode/{barcode}")]
        public async Task<IActionResult> GetByBarcode(string barcode, CancellationToken ct)
        {
            var result = await _sender.Send(new GetProductByBarcodeQuery(barcode), ct);
            if (result.IsFailure)
                return NotFound(result.Error);

            return Ok(result.Value);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] GetProductsQuery query, CancellationToken ct)
        {
            var result = await _sender.Send(query, ct);
            if (result.IsFailure)
                return BadRequest(result.Error);

            return Ok(result.Value);
        }

        [HttpPost("import-excel")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ImportExcel(IFormFile file, [FromQuery] bool updateExisting = false, CancellationToken ct = default)
        {
            if (file == null || file.Length == 0)
                return BadRequest("لم يتم تحديد ملف إكسيل مرفوع.");

            using var ms = new MemoryStream();
            await file.CopyToAsync(ms, ct);
            var fileBytes = ms.ToArray();

            var command = new ImportProductsFromExcelCommand(fileBytes, updateExisting);
            var result = await _sender.Send(command, ct);

            if (result.IsFailure)
                return BadRequest(result.Error);

            return Ok(result.Value);
        }

        [HttpGet("excel-template")]
        public async Task<IActionResult> DownloadTemplate(CancellationToken ct)
        {
            var result = await _sender.Send(new GetProductExcelTemplateQuery(), ct);
            if (result.IsFailure)
                return BadRequest(result.Error);

            return File(
                result.Value,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Products_Import_Template.xlsx");
        }
    }
}
