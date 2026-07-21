using FusionOS.Modules.Warehouse.Application.Racks.Commands.CreateRack;
using FusionOS.Modules.Warehouse.Application.Racks.Commands.DeactivateRack;
using FusionOS.Modules.Warehouse.Application.Racks.Commands.UpdateRack;
using FusionOS.Modules.Warehouse.Application.Racks.Queries.GetRackById;
using FusionOS.Modules.Warehouse.Application.Racks.Queries.ListRacks;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Warehouse.Api.Controllers;

/// <summary>Racks — optional level between Zone and Shelf/Bin. Nested under warehouses/zones, one level deeper than ZonesController, same shape as BinsController.</summary>
[ApiController]
[Route("api/v1/warehouse/warehouses/{warehouseId:guid}/zones/{zoneId:guid}/racks")]
public sealed class RacksController : ControllerBase
{
    private readonly ISender _sender;

    public RacksController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create(Guid warehouseId, Guid zoneId, [FromBody] CreateRackRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateRackCommand(request.CompanyId, zoneId, request.Name, request.Code);
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { warehouseId, zoneId, id = result.Id, companyId = request.CompanyId }, result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid warehouseId, Guid zoneId, Guid id, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetRackByIdQuery(companyId, id), cancellationToken);
        if (result is null || result.ZoneId != zoneId)
            return NotFound();
        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List(Guid warehouseId, Guid zoneId, [FromQuery] Guid companyId, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListRacksQuery(companyId, zoneId, page, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Update(Guid warehouseId, Guid zoneId, Guid id, [FromBody] UpdateRackRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateRackCommand(request.CompanyId, id, request.Name);
        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }

    // Soft-deactivate only — never a DELETE, this never removes the row.
    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Deactivate(Guid warehouseId, Guid zoneId, Guid id, [FromBody] DeactivateRackRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeactivateRackCommand(request.CompanyId, id), cancellationToken);
        return Ok(result);
    }
}

public sealed record CreateRackRequest(Guid CompanyId, string Name, string Code);
public sealed record UpdateRackRequest(Guid CompanyId, string Name);
public sealed record DeactivateRackRequest(Guid CompanyId);
