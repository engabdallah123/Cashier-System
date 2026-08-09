using Dashboard.Application.Dashboard.Queries.GetDashboard;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace POS.WebAPI.Controllers.Dashboard
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly IMediator _sender;

        public DashboardController(IMediator sender)
        {
            _sender = sender;
        }

        [HttpGet]
        public async Task<IActionResult> Get(
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            CancellationToken ct = default)
        {
            var result = await _sender.Send(new GetDashboardQuery(fromDate, toDate), ct);
            if (result.IsFailure)
                return BadRequest(result.Error);

            return Ok(result.Value);
        }
    }
}
