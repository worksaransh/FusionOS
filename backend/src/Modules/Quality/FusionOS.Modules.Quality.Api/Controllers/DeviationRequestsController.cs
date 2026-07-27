using FusionOS.Modules.Quality.Application.Deviations.Commands.ApproveDeviationRequest;
using FusionOS.Modules.Quality.Application.Deviations.Commands.CreateDeviationRequest;
using FusionOS.Modules.Quality.Application.Deviations.Commands.RejectDeviationRequest;
using FusionOS.Modules.Quality.Application.Deviations.Queries.GetDeviationRequestById;
using FusionOS.Modules.Quality.Application.Deviations.Queries.ListDeviationRequests;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Quality.Api.Controllers;

/// <summary>
/// Phase 5 — Quality: deviation/waiver requests — accepting an out-of-spec inspection result
/// (or a known-in-advance planned deviation) anyway, subject to a separate approver's decision.
/// Pending -> Approved or Pending -> Rejected, both terminal.
/// </summary>
[ApiController]
[Route("api/v1/quality/deviations")]
public sealed class DeviationRequestsController : ControllerBase
{
    private readonly ISender _sender;

    public DeviationRequestsController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateDeviationRequestRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateDeviationRequestCommand(request.CompanyId, request.InspectionId, request.Reason, request.RequestedByUserId);
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id, companyId = request.CompanyId }, result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetDeviationRequestByIdQuery(companyId, id), cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] Guid companyId, [FromQuery] Guid? inspectionId = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListDeviationRequestsQuery(companyId, inspectionId, page, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/approve")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Approve(Guid id, [FromBody] DeviationRequestDecisionRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ApproveDeviationRequestCommand(request.CompanyId, id, request.DecidedByUserId, request.Note), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/reject")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reject(Guid id, [FromBody] DeviationRequestDecisionRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new RejectDeviationRequestCommand(request.CompanyId, id, request.DecidedByUserId, request.Note), cancellationToken);
        return Ok(result);
    }
}

public sealed record CreateDeviationRequestRequest(Guid CompanyId, Guid? InspectionId, string Reason, Guid RequestedByUserId);

public sealed record DeviationRequestDecisionRequest(Guid CompanyId, Guid DecidedByUserId, string? Note);
