using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Ai.Api.Controllers;

/// <summary>Reports genuine module status — Ai has real Recommendation/DemandForecast/OcrJob/Search depth (though OCR still has no real extraction provider wired in), not just a registration probe.</summary>
[ApiController]
[Route("api/v1/ai/health")]
public sealed class AiHealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new
    {
        module = "Ai",
        status = "active",
        roadmapPhase = "Phase 7 — AI Platform",
        slices = new[] { "recommendations", "demand-forecasting", "ocr-jobs (no provider wired in)", "search" },
    });
}
