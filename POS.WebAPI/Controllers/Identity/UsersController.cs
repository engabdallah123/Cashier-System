using Identity.Application.Auth.Commands.Register;
using Identity.Application.Users.Commands.ActivateUser;
using Identity.Application.Users.Commands.DeactivateUser;
using Identity.Application.Users.Commands.UpdateUserRole;
using Identity.Application.Users.Queries.GetUserById;
using Identity.Application.Users.Queries.GetUsers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace POS.WebAPI.Controllers.Identity
{
    public record UpdateRoleRequest(string Role);

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

        [HttpPost]
        [Authorize(Roles = "Admin,Manager,Administrator")]
        public async Task<IActionResult> Create([FromBody] RegisterCommand command, CancellationToken ct)
        {
            var result = await _sender.Send(command, ct);
            if (result.IsFailure)
                return BadRequest(result.Error);

            return Ok(result.Value);
        }

        [HttpPut("{id}/role")]
        [Authorize(Roles = "Admin,Manager,Administrator")]
        public async Task<IActionResult> UpdateRole(string id, [FromBody] UpdateRoleRequest request, CancellationToken ct)
        {
            var command = new UpdateUserRoleCommand(id, request.Role);
            var result = await _sender.Send(command, ct);
            if (result.IsFailure)
                return BadRequest(result.Error);

            return NoContent();
        }

        [HttpPut("{id}/activate")]
        [Authorize(Roles = "Admin,Manager,Administrator")]
        public async Task<IActionResult> Activate(string id, CancellationToken ct)
        {
            var result = await _sender.Send(new ActivateUserCommand(id), ct);
            if (result.IsFailure)
                return BadRequest(result.Error);

            return NoContent();
        }

        [HttpPut("{id}/deactivate")]
        [Authorize(Roles = "Admin,Manager,Administrator")]
        public async Task<IActionResult> Deactivate(string id, CancellationToken ct)
        {
            var result = await _sender.Send(new DeactivateUserCommand(id), ct);
            if (result.IsFailure)
                return BadRequest(result.Error);

            return NoContent();
        }
    }
}
