using Inventory.Application.Catalog.Brands.Commands.CreateBrand;
using Inventory.Application.Catalog.Brands.Commands.DeleteBrand;
using Inventory.Application.Catalog.Brands.Commands.UpdateBrand;
using Inventory.Application.Catalog.Brands.Queries.GetBrands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace POS.WebAPI.Controllers.Inventory
{
    [ApiController]
    [Route("api/inventory/[controller]")]
    public class BrandsController : ControllerBase
    {
        private readonly IMediator _sender;

        public BrandsController(IMediator sender)
        {
            _sender = sender;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateBrandCommand command, CancellationToken ct)
        {
            var result = await _sender.Send(command, ct);
            if (result.IsFailure)
                return BadRequest(result.Error);

            return Ok(result.Value);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBrandCommand command, CancellationToken ct)
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
            var result = await _sender.Send(new DeleteBrandCommand(id), ct);
            if (result.IsFailure)
                return BadRequest(result.Error);

            return NoContent();
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] bool? onlyActive, CancellationToken ct)
        {
            var result = await _sender.Send(new GetBrandsQuery(onlyActive), ct);
            if (result.IsFailure)
                return BadRequest(result.Error);

            return Ok(result.Value);
        }
    }
}
