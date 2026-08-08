using Inventory.Application.Pricing.PriceLists.Commands.CreatePriceList;
using Inventory.Application.Pricing.PriceLists.Commands.DeletePriceList;
using Inventory.Application.Pricing.PriceLists.Commands.UpdatePriceList;
using Inventory.Application.Pricing.PriceLists.Queries.GetPriceLists;
using Inventory.Application.Pricing.ProductPrices.Commands.SetProductPrice;
using Inventory.Application.Pricing.ProductPrices.Queries.GetProductPrices;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace POS.WebAPI.Controllers.Inventory
{
    [ApiController]
    [Route("api/inventory/[controller]")]
    public class PriceListsController : ControllerBase
    {
        private readonly IMediator _sender;

        public PriceListsController(IMediator sender)
        {
            _sender = sender;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePriceListCommand command, CancellationToken ct)
        {
            var result = await _sender.Send(command, ct);
            if (result.IsFailure)
                return BadRequest(result.Error);

            return Ok(result.Value);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePriceListCommand command, CancellationToken ct)
        {
            if (id != command.Id)
                return BadRequest("ID in URL does not match request body.");

            var result = await _sender.Send(command, ct);
            if (result.IsFailure)
                return BadRequest(result.Error);

            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            var result = await _sender.Send(new DeletePriceListCommand(id), ct);
            if (result.IsFailure)
                return BadRequest(result.Error);

            return NoContent();
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] bool? onlyActive, CancellationToken ct)
        {
            var result = await _sender.Send(new GetPriceListsQuery(onlyActive), ct);
            if (result.IsFailure)
                return BadRequest(result.Error);

            return Ok(result.Value);
        }

        [HttpPost("prices")]
        public async Task<IActionResult> SetPrice([FromBody] SetProductPriceCommand command, CancellationToken ct)
        {
            var result = await _sender.Send(command, ct);
            if (result.IsFailure)
                return BadRequest(result.Error);

            return Ok(result.Value);
        }

        [HttpGet("prices")]
        public async Task<IActionResult> GetPrices([FromQuery] GetProductPricesQuery query, CancellationToken ct)
        {
            var result = await _sender.Send(query, ct);
            if (result.IsFailure)
                return BadRequest(result.Error);

            return Ok(result.Value);
        }
    }
}
