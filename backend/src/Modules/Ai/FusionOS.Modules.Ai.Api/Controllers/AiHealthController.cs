using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Ai.Api.Controllers;

/// <summary>The only endpoint this not-yet-implemented module exposes: proof it is registered and reachable.</summary>
[ApiController]
[Route("api/v1/ai/health")]
public sealed class AiHealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new
    {
        module = "Ai",
        status = "scaffolded",
        roadmapPhase = "Phase 7 — AI Platform",
    });
}
