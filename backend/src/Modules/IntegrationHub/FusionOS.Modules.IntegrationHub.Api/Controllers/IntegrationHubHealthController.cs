using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.IntegrationHub.Api.Controllers;

/// <summary>The only endpoint this not-yet-implemented module exposes: proof it is registered and reachable.</summary>
[ApiController]
[Route("api/v1/integration_hub/health")]
public sealed class IntegrationHubHealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new
    {
        module = "IntegrationHub",
        status = "scaffolded",
        roadmapPhase = "Phase 9 — Integrations & Mobile",
    });
}
