using Inventory.Application.Stock.StockTransfers.Commands.CreateStockTransfer;
using Inventory.Application.Stock.StockTransfers.Commands.ExecuteStockTransfer;
using Inventory.Application.Stock.StockTransfers.Queries.GetStockTransfers;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace POS.WebAPI.Controllers.Inventory
{
    [ApiController]
    [Route("api/inventory/[controller]")]
    public class StockTransfersController : ControllerBase
    {
        private readonly IMediator _sender;

        public StockTransfersController(IMediator sender)
        {
            _sender = sender;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateStockTransferCommand command, CancellationToken ct)
        {
            var result = await _sender.Send(command, ct);
            if (result.IsFailure)
                return BadRequest(result.Error);

            return Ok(result.Value);
        }

        [HttpPost("{id:guid}/execute")]
        public async Task<IActionResult> Execute(Guid id, [FromBody] string executedBy, CancellationToken ct)
        {
            var result = await _sender.Send(new ExecuteStockTransferCommand(id, executedBy), ct);
            if (result.IsFailure)
                return BadRequest(result.Error);

            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] GetStockTransfersQuery query, CancellationToken ct)
        {
            var result = await _sender.Send(query, ct);
            if (result.IsFailure)
                return BadRequest(result.Error);

            return Ok(result.Value);
        }
    }
}
