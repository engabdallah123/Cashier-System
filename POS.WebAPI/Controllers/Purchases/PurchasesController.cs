using MediatR;
using Microsoft.AspNetCore.Mvc;
using Purchases.Application.Purchases.Commands.CreatePurchase;
using Purchases.Application.Purchases.Commands.ReceivePurchase;
using Purchases.Application.Purchases.Queries.GetPurchases;

using Purchases.Application.Purchases.Commands.PayPurchaseInvoice;

namespace POS.WebAPI.Controllers.Purchases
{
    [ApiController]
    [Route("api/[controller]")]
    public class PurchasesController : ControllerBase
    {
        private readonly IMediator _sender;

        public PurchasesController(IMediator sender)
        {
            _sender = sender;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePurchaseCommand command, CancellationToken ct)
        {
            var result = await _sender.Send(command, ct);
            if (result.IsFailure)
                return BadRequest(result.Error);

            return Ok(result.Value);
        }

        [HttpPost("{id:guid}/receive")]
        public async Task<IActionResult> Receive(Guid id, [FromBody] Guid userId, CancellationToken ct)
        {
            var result = await _sender.Send(new ReceivePurchaseCommand(id, userId), ct);
            if (result.IsFailure)
                return BadRequest(result.Error);

            return NoContent();
        }

        [HttpPost("{id:guid}/pay")]
        public async Task<IActionResult> Pay(Guid id, [FromBody] decimal amount, CancellationToken ct)
        {
            var result = await _sender.Send(new PayPurchaseInvoiceCommand(id, amount), ct);
            if (result.IsFailure)
                return BadRequest(result.Error);

            return NoContent();
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] Guid? supplierId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50, CancellationToken ct = default)
        {
            var result = await _sender.Send(new GetPurchasesQuery(supplierId, page, pageSize), ct);
            if (result.IsFailure)
                return BadRequest(result.Error);

            return Ok(result.Value);
        }
    }
}
