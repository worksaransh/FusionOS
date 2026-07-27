using FusionOS.Modules.Inventory.Application.SerialUnits.Commands.RegisterSerialUnit;
using FusionOS.Modules.Inventory.Application.SerialUnits.Commands.UpdateSerialUnitStatus;
using FusionOS.Modules.Inventory.Application.SerialUnits.Queries.GetSerialUnitById;
using FusionOS.Modules.Inventory.Application.SerialUnits.Queries.GetSerialUnitBySerialNumber;
using FusionOS.Modules.Inventory.Application.SerialUnits.Queries.ListSerialUnitsByProduct;
using FusionOS.Modules.Inventory.Domain.SerialUnits;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Inventory.Api.Controllers;

/// <summary>
/// Structured serial-number tracking, alongside (not replacing) the existing
/// opaque InventoryLedgerEntry.SerialNumber/GoodsReceiptLine.SerialNumber
/// string fields — see SerialUnit.cs's doc comment. The "lookup" action is
/// the real "scan a serial and find it" use case.
/// </summary>
[ApiController]
[Route("api/v1/inventory/serial-units")]
public sealed class SerialUnitsController : ControllerBase
{
    private readonly ISender _sender;

    public SerialUnitsController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Register([FromBody] RegisterSerialUnitRequest request, CancellationToken cancellationToken)
    {
        var command = new RegisterSerialUnitCommand(request.CompanyId, request.ProductId, request.SerialNumber);
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id, companyId = request.CompanyId }, result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetSerialUnitByIdQuery(companyId, id), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    // The real "scan a serial and find it" use case — exact match, company-wide, not scoped to a single product.
    [HttpGet("lookup")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Lookup([FromQuery] Guid companyId, [FromQuery] string serialNumber, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetSerialUnitBySerialNumberQuery(companyId, serialNumber), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        [FromQuery] Guid companyId,
        [FromQuery] Guid productId,
        [FromQuery] SerialUnitStatus? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListSerialUnitsByProductQuery(companyId, productId, status, page, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateSerialUnitStatusRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new UpdateSerialUnitStatusCommand(request.CompanyId, id, request.NewStatus), cancellationToken);
        return Ok(result);
    }
}

public sealed record RegisterSerialUnitRequest(Guid CompanyId, Guid ProductId, string SerialNumber);

public sealed record UpdateSerialUnitStatusRequest(Guid CompanyId, SerialUnitStatus NewStatus);
