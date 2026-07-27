using FusionOS.Modules.Ai.Application.Search.Contracts;
using FusionOS.Modules.Ai.Application.Search.Queries.RunAiSearch;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Ai.Api.Controllers;

/// <summary>
/// Phase 7/8 — AI Platform: natural-language search, a translation layer over
/// existing RBAC-scoped read queries (12_AI_PLATFORM.md §4) — see
/// <see cref="FusionOS.Modules.Ai.Application.Search.AiSearchIntentResolver"/> for the exact,
/// documented keyword matching this endpoint uses instead of a real NLP model.
/// </summary>
[ApiController]
[Route("api/v1/ai/search")]
public sealed class AiSearchController : ControllerBase
{
    private readonly ISender _sender;

    public AiSearchController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AiSearchResultDto>> Search([FromBody] AiSearchRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new RunAiSearchQuery(request.CompanyId, request.QueryText), cancellationToken);
        return Ok(result);
    }
}

public sealed record AiSearchRequest(Guid CompanyId, string QueryText);
