using FusionOS.Modules.Manufacturing.Application.Machines.Commands.CreateMachine;
using FusionOS.Modules.Manufacturing.Application.Machines.Commands.DeactivateMachine;
using FusionOS.Modules.Manufacturing.Application.Machines.Commands.UpdateMachine;
using FusionOS.Modules.Manufacturing.Application.Machines.Queries.ListMachinesByWorkCenter;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Manufacturing.Api.Controllers;

/// <summary>
/// Machine Scheduling support (Phase 6, 2026-07-22): Machines. Master data describing a
/// piece of equipment belonging to exactly one WorkCenter — CRUD-ish shape (create/list-
/// by-work-center/soft-deactivate) mirroring WorkCentersController.
/// </summary>
[ApiController]
[Route("api/v1/manufacturing/machines")]
public sealed class MachinesController : ControllerBase
{
    private readonly ISender _sender;

    public MachinesController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateMachineRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateMachineCommand(request.CompanyId, request.Code, request.Name, request.WorkCenterId);
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(List), new { companyId = request.CompanyId, workCenterId = request.WorkCenterId }, result);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] Guid companyId, [FromQuery] Guid workCenterId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ListMachinesByWorkCenterQuery(companyId, workCenterId), cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMachineRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateMachineCommand(request.CompanyId, id, request.Name, request.WorkCenterId);
        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(Guid id, [FromBody] DeactivateMachineRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeactivateMachineCommand(request.CompanyId, id), cancellationToken);
        return Ok(result);
    }
}

public sealed record CreateMachineRequest(Guid CompanyId, string Code, string Name, Guid WorkCenterId);

public sealed record UpdateMachineRequest(Guid CompanyId, string Name, Guid WorkCenterId);

public sealed record DeactivateMachineRequest(Guid CompanyId);
