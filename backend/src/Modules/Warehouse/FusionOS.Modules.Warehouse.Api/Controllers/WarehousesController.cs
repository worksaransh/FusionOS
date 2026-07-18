using FusionOS.Modules.Warehouse.Application.Warehouses.Commands.CreateWarehouse;
using FusionOS.Modules.Warehouse.Application.Warehouses.Commands.DeactivateWarehouse;
using FusionOS.Modules.Warehouse.Application.Warehouses.Commands.UpdateWarehouse;
using FusionOS.Modules.Warehouse.Application.Warehouses.Queries.GetWarehouseById;
using FusionOS.Modules.Warehouse.Application.Warehouses.Queries.ListWarehouses;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Warehouse.Api.Controllers;

[ApiController]
[Route("api/v1/warehouse/warehouses")]
public sealed class WarehousesController : ControllerBase
{
    private readonly ISender _sender;

    public WarehousesController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateWarehouseCommand command, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    // Wired up (2026-07-14) — was a dead stub that always returned NotFound;
    // see CompaniesController for the same documented gap this fixes.
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetWarehouseByIdQuery(companyId, id), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] Guid companyId, [FromQuery] string? search = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListWarehousesQuery(companyId, search, page, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateWarehouseRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateWarehouseCommand(request.CompanyId, id, request.BranchId, request.Name, request.Address);
        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }

    // Soft-deactivate only — never a DELETE, this never removes the row
    // (08_API_STANDARDS.md / 04_DATABASE_GUIDELINES.md).
    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Deactivate(Guid id, [FromBody] DeactivateWarehouseRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeactivateWarehouseCommand(request.CompanyId, id), cancellationToken);
        return Ok(result);
    }
}

/// <summary>Request body for warehouse update — Id comes from the route, not the body.</summary>
public sealed record UpdateWarehouseRequest(Guid CompanyId, Guid? BranchId, string Name, string? Address);

/// <summary>Request body for warehouse deactivation — just carries CompanyId for tenant scoping.</summary>
public sealed record DeactivateWarehouseRequest(Guid CompanyId);
