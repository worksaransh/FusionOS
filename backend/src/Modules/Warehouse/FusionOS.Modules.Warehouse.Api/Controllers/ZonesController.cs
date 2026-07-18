using FusionOS.Modules.Warehouse.Application.Zones.Commands.CreateZone;
using FusionOS.Modules.Warehouse.Application.Zones.Commands.DeactivateZone;
using FusionOS.Modules.Warehouse.Application.Zones.Commands.UpdateZone;
using FusionOS.Modules.Warehouse.Application.Zones.Queries.GetZoneById;
using FusionOS.Modules.Warehouse.Application.Zones.Queries.ListZones;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Warehouse.Api.Controllers;

/// <summary>Zones — next slice after Warehouse (08_API_STANDARDS.md). Nested under warehouses per 08_API_STANDARDS.md §3.</summary>
[ApiController]
[Route("api/v1/warehouse/warehouses/{warehouseId:guid}/zones")]
public sealed class ZonesController : ControllerBase
{
    private readonly ISender _sender;

    public ZonesController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create(Guid warehouseId, [FromBody] CreateZoneRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateZoneCommand(request.CompanyId, warehouseId, request.Name, request.Code);
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { warehouseId, id = result.Id, companyId = request.CompanyId }, result);
    }

    // Zones previously had no GetById route at all (unlike Product/Warehouse's
    // dead stubs) — added 2026-07-14 alongside the same fix for those controllers.
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid warehouseId, Guid id, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetZoneByIdQuery(companyId, id), cancellationToken);
        if (result is null || result.WarehouseId != warehouseId)
            return NotFound();
        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List(Guid warehouseId, [FromQuery] Guid companyId, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListZonesQuery(companyId, warehouseId, page, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Update(Guid warehouseId, Guid id, [FromBody] UpdateZoneRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateZoneCommand(request.CompanyId, id, request.Name);
        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }

    // Soft-deactivate only — never a DELETE, this never removes the row
    // (08_API_STANDARDS.md / 04_DATABASE_GUIDELINES.md).
    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Deactivate(Guid warehouseId, Guid id, [FromBody] DeactivateZoneRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeactivateZoneCommand(request.CompanyId, id), cancellationToken);
        return Ok(result);
    }
}

/// <summary>Request body for zone creation — WarehouseId comes from the route, not the body, per REST convention.</summary>
public sealed record CreateZoneRequest(Guid CompanyId, string Name, string Code);

/// <summary>Request body for zone update — Id comes from the route, not the body.</summary>
public sealed record UpdateZoneRequest(Guid CompanyId, string Name);

/// <summary>Request body for zone deactivation — just carries CompanyId for tenant scoping.</summary>
public sealed record DeactivateZoneRequest(Guid CompanyId);
