using FusionOS.Modules.Ai.Application.Recommendations.Commands.AcceptRecommendation;
using FusionOS.Modules.Ai.Application.Recommendations.Commands.DismissRecommendation;
using FusionOS.Modules.Ai.Application.Recommendations.Commands.RecordRecommendation;
using FusionOS.Modules.Ai.Application.Recommendations.Queries.GetRecommendationById;
using FusionOS.Modules.Ai.Application.Recommendations.Queries.ListRecommendations;
using FusionOS.Modules.Ai.Domain.Recommendations;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Ai.Api.Controllers;

/// <summary>
/// Phase 7 — AI Platform: recommendations/insights, Pending → Accepted/Dismissed —
/// the human-in-the-loop record 12_AI_PLATFORM.md §3/§5 describes.
/// </summary>
[ApiController]
[Route("api/v1/ai/recommendations")]
public sealed class RecommendationsController : ControllerBase
{
    private readonly ISender _sender;

    public RecommendationsController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Record([FromBody] RecordRecommendationRequest request, CancellationToken cancellationToken)
    {
        var command = new RecordRecommendationCommand(request.CompanyId, request.Type, request.ReferenceId, request.Summary, request.ModelVersion);
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id, companyId = request.CompanyId }, result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetRecommendationByIdQuery(companyId, id), cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] Guid companyId, [FromQuery] RecommendationStatus? status = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListRecommendationsQuery(companyId, status, page, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/accept")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Accept(Guid id, [FromBody] RecommendationActionRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new AcceptRecommendationCommand(request.CompanyId, id), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/dismiss")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Dismiss(Guid id, [FromBody] RecommendationActionRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DismissRecommendationCommand(request.CompanyId, id), cancellationToken);
        return Ok(result);
    }
}

public sealed record RecordRecommendationRequest(Guid CompanyId, string Type, Guid ReferenceId, string Summary, string ModelVersion);

public sealed record RecommendationActionRequest(Guid CompanyId);
