using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Maintenance.Api.Controllers;

/// <summary>The only endpoint this not-yet-implemented module exposes: proof it is registered and reachable.</summary>
[ApiController]
[Route("api/v1/maintenance/health")]
public sealed class MaintenanceHealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new
    {
        module = "Maintenance",
        status = "scaffolded",
        roadmapPhase = "Phase 5 — Quality & Maintenance",
    });
}
