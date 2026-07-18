using FusionOS.Modules.Inventory.Application.Reservations.Commands.CreateReservation;
using FusionOS.Modules.Inventory.Application.Reservations.Commands.FulfillReservation;
using FusionOS.Modules.Inventory.Application.Reservations.Commands.ReleaseReservation;
using FusionOS.Modules.Inventory.Application.Reservations.Queries.GetAvailableToPromise;
using FusionOS.Modules.Inventory.Application.Reservations.Queries.ListReservations;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Inventory.Api.Controllers;

/// <summary>
/// Phase 1 closeout: soft stock holds against a Product/Warehouse, Active → Released/
/// Fulfilled. No GetById — no controller action needs a single reservation, only list
/// and the available-to-promise roll-up, same simplification Marketplace's
/// PluginInstallationsController makes.
/// </summary>
[ApiController]
[Route("api/v1/inventory/reservations")]
public sealed class ReservationsController : ControllerBase
{
    private readonly ISender _sender;

    public ReservationsController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateReservationRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateReservationCommand(request.CompanyId, request.ProductId, request.WarehouseId, request.Quantity, request.ReferenceType, request.ReferenceId);
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(List), new { companyId = request.CompanyId }, result);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] Guid companyId, [FromQuery] Guid? productId = null, [FromQuery] Guid? warehouseId = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListReservationsQuery(companyId, productId, warehouseId, page, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpGet("available-to-promise")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAvailableToPromise([FromQuery] Guid companyId, [FromQuery] Guid productId, [FromQuery] Guid warehouseId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetAvailableToPromiseQuery(companyId, productId, warehouseId), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/release")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Release(Guid id, [FromBody] ReservationActionRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ReleaseReservationCommand(request.CompanyId, id), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/fulfill")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Fulfill(Guid id, [FromBody] ReservationActionRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new FulfillReservationCommand(request.CompanyId, id), cancellationToken);
        return Ok(result);
    }
}

public sealed record CreateReservationRequest(Guid CompanyId, Guid ProductId, Guid WarehouseId, decimal Quantity, string ReferenceType, Guid ReferenceId);

public sealed record ReservationActionRequest(Guid CompanyId);
