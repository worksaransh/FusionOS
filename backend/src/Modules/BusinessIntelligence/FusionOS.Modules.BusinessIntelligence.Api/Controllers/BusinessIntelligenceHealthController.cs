using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.BusinessIntelligence.Api.Controllers;

/// <summary>The only endpoint this not-yet-implemented module exposes: proof it is registered and reachable.</summary>
[ApiController]
[Route("api/v1/bi/health")]
public sealed class BusinessIntelligenceHealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new
    {
        module = "BusinessIntelligence",
        status = "scaffolded",
        roadmapPhase = "Phase 6 — Business Intelligence",
    });
}
