using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Quality.Api.Controllers;

/// <summary>The only endpoint this not-yet-implemented module exposes: proof it is registered and reachable.</summary>
[ApiController]
[Route("api/v1/quality/health")]
public sealed class QualityHealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new
    {
        module = "Quality",
        status = "scaffolded",
        roadmapPhase = "Phase 5 — Quality & Maintenance",
    });
}
