using FusionOS.Modules.Warehouse.Application.Shelves.Commands.CreateShelf;
using FusionOS.Modules.Warehouse.Application.Shelves.Commands.DeactivateShelf;
using FusionOS.Modules.Warehouse.Application.Shelves.Commands.UpdateShelf;
using FusionOS.Modules.Warehouse.Application.Shelves.Queries.GetShelfById;
using FusionOS.Modules.Warehouse.Application.Shelves.Queries.ListShelves;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Warehouse.Api.Controllers;

/// <summary>Shelves — optional level nested under Rack, one level deeper than RacksController, same shape as BinsController/RacksController.</summary>
[ApiController]
[Route("api/v1/warehouse/warehouses/{warehouseId:guid}/zones/{zoneId:guid}/racks/{rackId:guid}/shelves")]
public sealed class ShelvesController : ControllerBase
{
    private readonly ISender _sender;

    public ShelvesController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create(Guid warehouseId, Guid zoneId, Guid rackId, [FromBody] CreateShelfRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateShelfCommand(request.CompanyId, rackId, request.Name, request.Code);
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { warehouseId, zoneId, rackId, id = result.Id, companyId = request.CompanyId }, result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid warehouseId, Guid zoneId, Guid rackId, Guid id, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetShelfByIdQuery(companyId, id), cancellationToken);
        if (result is null || result.RackId != rackId)
            return NotFound();
        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List(Guid warehouseId, Guid zoneId, Guid rackId, [FromQuery] Guid companyId, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListShelvesQuery(companyId, rackId, page, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Update(Guid warehouseId, Guid zoneId, Guid rackId, Guid id, [FromBody] UpdateShelfRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateShelfCommand(request.CompanyId, id, request.Name);
        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }

    // Soft-deactivate only — never a DELETE, this never removes the row.
    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Deactivate(Guid warehouseId, Guid zoneId, Guid rackId, Guid id, [FromBody] DeactivateShelfRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeactivateShelfCommand(request.CompanyId, id), cancellationToken);
        return Ok(result);
    }
}

public sealed record CreateShelfRequest(Guid CompanyId, string Name, string Code);
public sealed record UpdateShelfRequest(Guid CompanyId, string Name);
public sealed record DeactivateShelfRequest(Guid CompanyId);
