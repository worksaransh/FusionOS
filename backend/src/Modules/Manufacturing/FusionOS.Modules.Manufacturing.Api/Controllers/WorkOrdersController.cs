using FusionOS.Modules.Manufacturing.Application.WorkOrders.Commands.CancelWorkOrder;
using FusionOS.Modules.Manufacturing.Application.WorkOrders.Commands.CompleteWorkOrder;
using FusionOS.Modules.Manufacturing.Application.WorkOrders.Commands.CompleteWorkOrderOperation;
using FusionOS.Modules.Manufacturing.Application.WorkOrders.Commands.CreateReworkWorkOrder;
using FusionOS.Modules.Manufacturing.Application.WorkOrders.Commands.CreateWorkOrder;
using FusionOS.Modules.Manufacturing.Application.WorkOrders.Commands.IssueMaterialToWorkOrder;
using FusionOS.Modules.Manufacturing.Application.WorkOrders.Commands.ReleaseWorkOrder;
using FusionOS.Modules.Manufacturing.Application.WorkOrders.Commands.ReturnMaterialFromWorkOrder;
using FusionOS.Modules.Manufacturing.Application.WorkOrders.Commands.ScheduleWorkOrderOperation;
using FusionOS.Modules.Manufacturing.Application.WorkOrders.Commands.StartWorkOrderOperation;
using FusionOS.Modules.Manufacturing.Application.WorkOrders.Queries.GetWorkOrderById;
using FusionOS.Modules.Manufacturing.Application.WorkOrders.Queries.ListWorkOrders;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Manufacturing.Api.Controllers;

/// <summary>
/// Phase 3 — Manufacturing ERP: Work Orders. An order to manufacture a quantity of a
/// product from a bill of materials, with a Draft → Released → Completed lifecycle.
/// Completing raises WorkOrderCompleted, which Inventory consumes to post the real
/// stock movements (consume components, produce the parent product).
/// </summary>
[ApiController]
[Route("api/v1/manufacturing/work-orders")]
public sealed class WorkOrdersController : ControllerBase
{
    private readonly ISender _sender;

    public WorkOrdersController(ISender sender) => _sender = sender;

    /// <summary>Creates a new Draft work order for the given bill of materials, warehouse and quantity to produce.</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateWorkOrderRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateWorkOrderCommand(request.CompanyId, request.BillOfMaterialsId, request.WarehouseId, request.QuantityToProduce);
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id, companyId = request.CompanyId }, result);
    }

    /// <summary>Gets a single work order by id, scoped to the given company.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetWorkOrderByIdQuery(companyId, id), cancellationToken);
        return Ok(result);
    }

    /// <summary>Lists work orders for a company, paginated (default page size 25).</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] Guid companyId, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListWorkOrdersQuery(companyId, page, pageSize), cancellationToken);
        return Ok(result);
    }

    /// <summary>Releases the (Draft) work order to the shop floor, snapshotting its BOM components and routing operations so later BOM/routing edits don't retroactively change it.</summary>
    [HttpPost("{id:guid}/release")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Release(Guid id, [FromBody] WorkOrderActionRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ReleaseWorkOrderCommand(request.CompanyId, id), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Completes the work order. QuantityGoodProduced/QuantityScrapped are optional
    /// scrap/yield recording — omit both for the default 100%-yield, no-scrap
    /// completion (the full planned QuantityToProduce, exactly as before).
    /// </summary>
    [HttpPost("{id:guid}/complete")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Complete(Guid id, [FromBody] CompleteWorkOrderRequest request, CancellationToken cancellationToken)
    {
        var command = new CompleteWorkOrderCommand(request.CompanyId, id, request.QuantityGoodProduced, request.QuantityScrapped);
        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>Cancels a not-yet-completed (Draft or Released) work order. A Completed work order cannot be cancelled — its stock movements have already posted.</summary>
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] WorkOrderActionRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new CancelWorkOrderCommand(request.CompanyId, id), cancellationToken);
        return Ok(result);
    }

    /// <summary>Issues (consumes on the shop floor) a quantity of one snapshotted component against a Released work order.</summary>
    [HttpPost("{id:guid}/materials/issue")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> IssueMaterial(Guid id, [FromBody] MaterialMovementRequest request, CancellationToken cancellationToken)
    {
        var command = new IssueMaterialToWorkOrderCommand(request.CompanyId, id, request.ComponentProductId, request.Quantity);
        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>Returns previously issued material for a component on a Released work order — the inverse of Issue.</summary>
    [HttpPost("{id:guid}/materials/return")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReturnMaterial(Guid id, [FromBody] MaterialMovementRequest request, CancellationToken cancellationToken)
    {
        var command = new ReturnMaterialFromWorkOrderCommand(request.CompanyId, id, request.ComponentProductId, request.Quantity);
        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>Starts one of this (Released) work order's snapshotted routing operations (Phase 6).</summary>
    [HttpPost("{id:guid}/operations/{operationId:guid}/start")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> StartOperation(Guid id, Guid operationId, [FromBody] WorkOrderActionRequest request, CancellationToken cancellationToken)
    {
        var command = new StartWorkOrderOperationCommand(request.CompanyId, id, operationId);
        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>Completes one of this (Released) work order's snapshotted routing operations (Phase 6). Does not itself complete the whole work order.</summary>
    [HttpPost("{id:guid}/operations/{operationId:guid}/complete")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CompleteOperation(Guid id, Guid operationId, [FromBody] WorkOrderActionRequest request, CancellationToken cancellationToken)
    {
        var command = new CompleteWorkOrderOperationCommand(request.CompanyId, id, operationId);
        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>Creates a new work order to rework a quantity of this (Completed) source work order's scrapped output (Rework, Phase 6). {id} is the source work order.</summary>
    [HttpPost("{id:guid}/rework")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateRework(Guid id, [FromBody] CreateReworkWorkOrderRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateReworkWorkOrderCommand(request.CompanyId, id, request.QuantityToRework);
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id, companyId = request.CompanyId }, result);
    }

    /// <summary>
    /// Machine Scheduling (Phase 6): assigns a machine and time window to one of this
    /// work order's snapshotted routing operations. Cross-work-order conflict checking
    /// (does the machine already have an overlapping schedule elsewhere) happens in
    /// ScheduleWorkOrderOperationCommandHandler — see its doc comment.
    /// </summary>
    [HttpPost("{id:guid}/operations/{operationId:guid}/schedule")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ScheduleOperation(Guid id, Guid operationId, [FromBody] ScheduleWorkOrderOperationRequest request, CancellationToken cancellationToken)
    {
        var command = new ScheduleWorkOrderOperationCommand(request.CompanyId, id, operationId, request.MachineId, request.ScheduledStart, request.ScheduledEnd);
        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }
}

public sealed record CreateWorkOrderRequest(Guid CompanyId, Guid BillOfMaterialsId, Guid WarehouseId, decimal QuantityToProduce);

public sealed record WorkOrderActionRequest(Guid CompanyId);

public sealed record CompleteWorkOrderRequest(Guid CompanyId, decimal? QuantityGoodProduced = null, decimal? QuantityScrapped = null);

public sealed record MaterialMovementRequest(Guid CompanyId, Guid ComponentProductId, decimal Quantity);

public sealed record CreateReworkWorkOrderRequest(Guid CompanyId, decimal QuantityToRework);

public sealed record ScheduleWorkOrderOperationRequest(Guid CompanyId, Guid MachineId, DateTimeOffset ScheduledStart, DateTimeOffset ScheduledEnd);
