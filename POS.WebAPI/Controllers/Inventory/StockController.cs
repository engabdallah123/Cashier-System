using Inventory.Application.Stock.StockBalances.Commands.AdjustStock;
using Inventory.Application.Stock.StockBalances.Queries.GetStockBalance;
using Inventory.Application.Stock.StockMovements.Queries.GetStockMovements;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace POS.WebAPI.Controllers.Inventory
{
    [ApiController]
    [Route("api/inventory/[controller]")]
    public class StockController : ControllerBase
    {
        private readonly IMediator   _sender;

        public StockController(IMediator sender)
        {
            _sender = sender;
        }

        [HttpPost("adjust")]
        public async Task<IActionResult> Adjust([FromBody] AdjustStockCommand command, CancellationToken ct)
        {
            var result = await _sender.Send(command, ct);
            if (result.IsFailure)
                return BadRequest(result.Error);

            return Ok();
        }

        [HttpGet("balances")]
        public async Task<IActionResult> GetBalances([FromQuery] GetStockBalanceQuery query, CancellationToken ct)
        {
            var result = await _sender.Send(query, ct);
            if (result.IsFailure)
                return BadRequest(result.Error);

            return Ok(result.Value);
        }

        [HttpGet("movements")]
        public async Task<IActionResult> GetMovements([FromQuery] GetStockMovementsQuery query, CancellationToken ct)
        {
            var result = await _sender.Send(query, ct);
            if (result.IsFailure)
                return BadRequest(result.Error);

            return Ok(result.Value);
        }
    }
}
