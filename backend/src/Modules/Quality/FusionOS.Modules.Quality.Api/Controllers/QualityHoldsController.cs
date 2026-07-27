using FusionOS.Modules.Quality.Application.QualityHolds.Commands.CreateQualityHold;
using FusionOS.Modules.Quality.Application.QualityHolds.Commands.ReleaseQualityHold;
using FusionOS.Modules.Quality.Application.QualityHolds.Queries.GetQualityHoldById;
using FusionOS.Modules.Quality.Application.QualityHolds.Queries.ListQualityHolds;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Quality.Api.Controllers;

/// <summary>
/// Phase 5 — Quality: Quality Holds, a hold/quarantine on some other module's record
/// (EntityType/EntityId, opaque — same convention as Core's ApprovalRequest), optionally
/// triggered by a failed Inspection (see RecordInspectionResultsCommand's RaiseHoldOnFailure).
/// </summary>
[ApiController]
[Route("api/v1/quality/quality-holds")]
public sealed class QualityHoldsController : ControllerBase
{
    private readonly ISender _sender;

    public QualityHoldsController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateQualityHoldRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateQualityHoldCommand(request.CompanyId, request.EntityType, request.EntityId, request.Reason, request.InspectionId);
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id, companyId = request.CompanyId }, result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetQualityHoldByIdQuery(companyId, id), cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        [FromQuery] Guid companyId,
        [FromQuery] string? entityType = null,
        [FromQuery] Guid? entityId = null,
        [FromQuery] bool? activeOnly = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListQualityHoldsQuery(companyId, entityType, entityId, activeOnly, page, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/release")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Release(Guid id, [FromBody] ReleaseQualityHoldRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ReleaseQualityHoldCommand(request.CompanyId, id, request.Note), cancellationToken);
        return Ok(result);
    }
}

public sealed record CreateQualityHoldRequest(Guid CompanyId, string EntityType, Guid EntityId, string Reason, Guid? InspectionId);

public sealed record ReleaseQualityHoldRequest(Guid CompanyId, string? Note);
