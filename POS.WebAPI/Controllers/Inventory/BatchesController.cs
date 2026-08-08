using Inventory.Application.Batches.ProductBatches.Commands.CreateProductBatch;
using Inventory.Application.Batches.ProductBatches.Commands.DeleteProductBatch;
using Inventory.Application.Batches.ProductBatches.Queries.GetExpiringBatches;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace POS.WebAPI.Controllers.Inventory
{
    [ApiController]
    [Route("api/inventory/[controller]")]
    public class BatchesController : ControllerBase
    {
        private readonly IMediator _sender;

        public BatchesController(IMediator sender)
        {
            _sender = sender;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProductBatchCommand command, CancellationToken ct)
        {
            var result = await _sender.Send(command, ct);
            if (result.IsFailure)
                return BadRequest(result.Error);

            return Ok(result.Value);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            var result = await _sender.Send(new DeleteProductBatchCommand(id), ct);
            if (result.IsFailure)
                return BadRequest(result.Error);

            return NoContent();
        }

        [HttpGet("expiring")]
        public async Task<IActionResult> GetExpiring([FromQuery] GetExpiringBatchesQuery query, CancellationToken ct)
        {
            var result = await _sender.Send(query, ct);
            if (result.IsFailure)
                return BadRequest(result.Error);

            return Ok(result.Value);
        }
    }
}
