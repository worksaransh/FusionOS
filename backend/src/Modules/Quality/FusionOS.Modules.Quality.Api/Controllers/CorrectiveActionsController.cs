using FusionOS.Modules.Quality.Application.CorrectiveActions.Commands.CloseCorrectiveAction;
using FusionOS.Modules.Quality.Application.CorrectiveActions.Commands.CreateCorrectiveAction;
using FusionOS.Modules.Quality.Application.CorrectiveActions.Commands.StartCorrectiveAction;
using FusionOS.Modules.Quality.Application.CorrectiveActions.Commands.VerifyCorrectiveAction;
using FusionOS.Modules.Quality.Application.CorrectiveActions.Queries.GetCorrectiveActionById;
using FusionOS.Modules.Quality.Application.CorrectiveActions.Queries.ListCorrectiveActions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Quality.Api.Controllers;

/// <summary>
/// Phase 5 — Quality: Corrective and Preventive Action (CAPA) plans raised against a
/// NonConformanceReport — root cause, corrective fix, preventive measure, assigned to a
/// user with a due date. Open -> InProgress -> Closed -> Verified.
/// </summary>
[ApiController]
[Route("api/v1/quality/corrective-actions")]
public sealed class CorrectiveActionsController : ControllerBase
{
    private readonly ISender _sender;

    public CorrectiveActionsController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateCorrectiveActionRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateCorrectiveActionCommand(
            request.CompanyId,
            request.NonConformanceReportId,
            request.RootCauseDescription,
            request.CorrectiveActionDescription,
            request.PreventiveActionDescription,
            request.AssignedToUserId,
            request.DueDate);
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id, companyId = request.CompanyId }, result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetCorrectiveActionByIdQuery(companyId, id), cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] Guid companyId, [FromQuery] Guid? nonConformanceReportId = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListCorrectiveActionsQuery(companyId, nonConformanceReportId, page, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/start")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Start(Guid id, [FromBody] CorrectiveActionActionRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new StartCorrectiveActionCommand(request.CompanyId, id), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/close")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Close(Guid id, [FromBody] CorrectiveActionActionRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new CloseCorrectiveActionCommand(request.CompanyId, id), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/verify")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Verify(Guid id, [FromBody] CorrectiveActionActionRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new VerifyCorrectiveActionCommand(request.CompanyId, id), cancellationToken);
        return Ok(result);
    }
}

public sealed record CreateCorrectiveActionRequest(
    Guid CompanyId,
    Guid NonConformanceReportId,
    string RootCauseDescription,
    string CorrectiveActionDescription,
    string PreventiveActionDescription,
    Guid AssignedToUserId,
    DateTimeOffset DueDate);

public sealed record CorrectiveActionActionRequest(Guid CompanyId);
