using MediatR;
using Microsoft.AspNetCore.Mvc;
using Settings.Application.StoreSettings.Commands.UpdateSettings;
using Settings.Application.StoreSettings.Queries.GetSettings;

namespace POS.WebAPI.Controllers.Settings
{
    [ApiController]
    [Route("api/[controller]")]
    public class SettingsController : ControllerBase
    {
        private readonly IMediator _sender;

        public SettingsController(IMediator sender)
        {
            _sender = sender;
        }

        [HttpGet]
        public async Task<IActionResult> Get(CancellationToken ct)
        {
            var result = await _sender.Send(new GetSettingsQuery(), ct);
            if (result.IsFailure)
                return NotFound(result.Error);

            return Ok(result.Value);
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateSettingsCommand command, CancellationToken ct)
        {
            var result = await _sender.Send(command, ct);
            if (result.IsFailure)
                return BadRequest(result.Error);

            return NoContent();
        }
    }
}
