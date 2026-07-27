using FusionOS.Modules.Ai.Application.Forecasting.Contracts;
using FusionOS.Modules.Ai.Application.Forecasting.Queries.GetDemandForecast;
using FusionOS.Modules.Ai.Application.Recommendations.Commands.GenerateReorderRecommendation;
using FusionOS.Modules.Ai.Application.Recommendations.Contracts;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Ai.Api.Controllers;

/// <summary>
/// Phase 8 — AI &amp; Integrations. The real, honest demand-forecast slice: a
/// deterministic linear-regression/moving-average calculation over real
/// dispatch history (see DemandHistoryEntry and GetDemandForecastQuery's own
/// doc comments), plus the endpoint that turns a forecast into a recorded
/// "ReorderSuggestion" Recommendation.
/// </summary>
[ApiController]
[Route("api/v1/ai/forecasts")]
public sealed class ForecastsController : ControllerBase
{
    private readonly ISender _sender;

    public ForecastsController(ISender sender) => _sender = sender;

    [HttpGet("{productId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetForecast(
        Guid productId,
        [FromQuery] Guid companyId,
        [FromQuery] int historyDays = 90,
        [FromQuery] int forecastDays = 30,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new GetDemandForecastQuery(companyId, productId, historyDays, forecastDays), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{productId:guid}/generate-reorder-recommendation")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GenerateReorderRecommendation(
        Guid productId,
        [FromBody] GenerateReorderRecommendationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GenerateReorderRecommendationCommand(request.CompanyId, productId), cancellationToken);

        // No Recommendation is created when the product isn't forecastable yet, or
        // when demand is essentially zero — an honest "nothing to suggest" rather
        // than a fabricated one, so this returns 204 rather than a fake 200 body.
        return result is RecommendationDto dto ? Ok(dto) : NoContent();
    }
}

public sealed record GenerateReorderRecommendationRequest(Guid CompanyId);
