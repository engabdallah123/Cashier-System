using Identity.Application.Users.Commands.ActivateUser;
using Identity.Application.Users.Commands.DeactivateUser;
using Identity.Application.Users.Queries.GetUserById;
using Identity.Application.Users.Queries.GetUsers;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace POS.WebAPI.Controllers.Identity
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IMediator _sender;

        public UsersController(IMediator sender)
        {
            _sender = sender;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var result = await _sender.Send(new GetUsersQuery(), ct);
            if (result.IsFailure)
                return BadRequest(result.Error);

            return Ok(result.Value);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id, CancellationToken ct)
        {
            var result = await _sender.Send(new GetUserByIdQuery(id), ct);
            if (result.IsFailure)
                return NotFound(result.Error);

            return Ok(result.Value);
        }

        [HttpPut("{id}/activate")]
        public async Task<IActionResult> Activate(string id, CancellationToken ct)
        {
            var result = await _sender.Send(new ActivateUserCommand(id), ct);
            if (result.IsFailure)
                return BadRequest(result.Error);

            return NoContent();
        }

        [HttpPut("{id}/deactivate")]
        public async Task<IActionResult> Deactivate(string id, CancellationToken ct)
        {
            var result = await _sender.Send(new DeactivateUserCommand(id), ct);
            if (result.IsFailure)
                return BadRequest(result.Error);

            return NoContent();
        }
    }
}
