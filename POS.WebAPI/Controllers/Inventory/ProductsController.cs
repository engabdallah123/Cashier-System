using Inventory.Application.Catalog.Products.Commands.CreateProduct;
using Inventory.Application.Catalog.Products.Commands.DeleteProduct;
using Inventory.Application.Catalog.Products.Commands.UpdateProduct;
using Inventory.Application.Catalog.Products.Queries.GetProductByBarcode;
using Inventory.Application.Catalog.Products.Queries.GetProductById;
using Inventory.Application.Catalog.Products.Queries.GetProducts;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace POS.WebAPI.Controllers.Inventory
{
    [ApiController]
    [Route("api/inventory/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IMediator _sender;

        public ProductsController(IMediator sender)
        {
            _sender = sender;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProductCommand command, CancellationToken ct)
        {
            var result = await _sender.Send(command, ct);
            if (result.IsFailure)
                return BadRequest(result.Error);

            return CreatedAtAction(nameof(GetById), new { id = result.Value }, result.Value);
        }

        [HttpPut("{id:guid}")]
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
    }
}
