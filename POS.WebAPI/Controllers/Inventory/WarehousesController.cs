using Inventory.Application.Stock.Warehouses.Commands.CreateWarehouse;
using Inventory.Application.Stock.Warehouses.Commands.DeleteWarehouse;
using Inventory.Application.Stock.Warehouses.Commands.UpdateWarehouse;
using Inventory.Application.Stock.Warehouses.Queries.GetWarehouses;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace POS.WebAPI.Controllers.Inventory
{
    [ApiController]
    [Route("api/inventory/[controller]")]
    public class WarehousesController : ControllerBase
    {
        private readonly IMediator _sender;

        public WarehousesController(IMediator sender)
        {
            _sender = sender;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateWarehouseCommand command, CancellationToken ct)
        {
            var result = await _sender.Send(command, ct);
            if (result.IsFailure)
                return BadRequest(result.Error);

            return Ok(result.Value);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateWarehouseCommand command, CancellationToken ct)
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
            var result = await _sender.Send(new DeleteWarehouseCommand(id), ct);
            if (result.IsFailure)
                return BadRequest(result.Error);

            return NoContent();
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] bool? onlyActive, CancellationToken ct)
        {
            var result = await _sender.Send(new GetWarehousesQuery(onlyActive), ct);
            if (result.IsFailure)
                return BadRequest(result.Error);

            return Ok(result.Value);
        }
    }
}
