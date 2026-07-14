using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Marketplace.Api.Controllers;

/// <summary>The only endpoint this not-yet-implemented module exposes: proof it is registered and reachable.</summary>
[ApiController]
[Route("api/v1/marketplace/health")]
public sealed class MarketplaceHealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new
    {
        module = "Marketplace",
        status = "scaffolded",
        roadmapPhase = "Phase 8 — Marketplace & Ecosystem",
    });
}
