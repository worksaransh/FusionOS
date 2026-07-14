using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Hrms.Api.Controllers;

/// <summary>The only endpoint this not-yet-implemented module exposes: proof it is registered and reachable.</summary>
[ApiController]
[Route("api/v1/hrms/health")]
public sealed class HrmsHealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new
    {
        module = "Hrms",
        status = "scaffolded",
        roadmapPhase = "Phase 4 — CRM & HRMS",
    });
}
