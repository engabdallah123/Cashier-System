using MediatR;
using Microsoft.AspNetCore.Mvc;
using Returns.Application.PurchaseReturns.Commands.CreatePurchaseReturn;
using Returns.Application.PurchaseReturns.Queries.GetPurchaseReturns;

namespace POS.WebAPI.Controllers.Returns
{
    [ApiController]
    [Route("api/returns/purchases")]
    public class PurchaseReturnsController : ControllerBase
    {
        private readonly IMediator _sender;

        public PurchaseReturnsController(IMediator sender)
        {
            _sender = sender;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePurchaseReturnCommand command, CancellationToken ct)
        {
            var result = await _sender.Send(command, ct);
            if (result.IsFailure)
                return BadRequest(result.Error);

            return Ok(result.Value);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] Guid? supplierId = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50,
            CancellationToken ct = default)
        {
            var result = await _sender.Send(new GetPurchaseReturnsQuery(supplierId, page, pageSize), ct);
            if (result.IsFailure)
                return BadRequest(result.Error);

            return Ok(result.Value);
        }
    }
}
