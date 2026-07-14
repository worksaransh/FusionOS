using FusionOS.Modules.Warehouse.Application.Zones.Commands.CreateZone;
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
        return CreatedAtAction(nameof(List), new { warehouseId, companyId = request.CompanyId }, result);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List(Guid warehouseId, [FromQuery] Guid companyId, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListZonesQuery(companyId, warehouseId, page, pageSize), cancellationToken);
        return Ok(result);
    }
}

/// <summary>Request body for zone creation — WarehouseId comes from the route, not the body, per REST convention.</summary>
public sealed record CreateZoneRequest(Guid CompanyId, string Name, string Code);
