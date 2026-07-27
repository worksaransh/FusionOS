using FusionOS.Modules.Quality.Application.Audits.Commands.CompleteQualityAudit;
using FusionOS.Modules.Quality.Application.Audits.Commands.CreateQualityAudit;
using FusionOS.Modules.Quality.Application.Audits.Commands.RecordAuditFinding;
using FusionOS.Modules.Quality.Application.Audits.Commands.ResolveAuditFinding;
using FusionOS.Modules.Quality.Application.Audits.Commands.StartQualityAudit;
using FusionOS.Modules.Quality.Application.Audits.Queries.GetQualityAuditById;
using FusionOS.Modules.Quality.Application.Audits.Queries.ListQualityAudits;
using FusionOS.Modules.Quality.Domain.Audits;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Quality.Api.Controllers;

/// <summary>
/// Phase 5 — Quality: scheduled internal/external quality audits, moving forward-only
/// Scheduled -> InProgress -> Completed, with zero or more findings recorded and
/// individually resolvable along the way.
/// </summary>
[ApiController]
[Route("api/v1/quality/audits")]
public sealed class QualityAuditsController : ControllerBase
{
    private readonly ISender _sender;

    public QualityAuditsController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateQualityAuditRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateQualityAuditCommand(request.CompanyId, request.AuditType, request.ScheduledDate, request.Auditor, request.Scope);
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id, companyId = request.CompanyId }, result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetQualityAuditByIdQuery(companyId, id), cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] Guid companyId, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListQualityAuditsQuery(companyId, page, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/start")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Start(Guid id, [FromBody] QualityAuditActionRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new StartQualityAuditCommand(request.CompanyId, id), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/complete")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Complete(Guid id, [FromBody] QualityAuditActionRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new CompleteQualityAuditCommand(request.CompanyId, id), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/findings")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RecordFinding(Guid id, [FromBody] RecordAuditFindingRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new RecordAuditFindingCommand(request.CompanyId, id, request.Description, request.Severity), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/findings/{findingId:guid}/resolve")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResolveFinding(Guid id, Guid findingId, [FromBody] QualityAuditActionRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ResolveAuditFindingCommand(request.CompanyId, id, findingId), cancellationToken);
        return Ok(result);
    }
}

public sealed record CreateQualityAuditRequest(Guid CompanyId, QualityAuditType AuditType, DateTimeOffset ScheduledDate, string Auditor, string Scope);

public sealed record QualityAuditActionRequest(Guid CompanyId);

public sealed record RecordAuditFindingRequest(Guid CompanyId, string Description, AuditFindingSeverity Severity);
