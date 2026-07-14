using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Crm.Api.Controllers;

/// <summary>The only endpoint this not-yet-implemented module exposes: proof it is registered and reachable.</summary>
[ApiController]
[Route("api/v1/crm/health")]
public sealed class CrmHealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new
    {
        module = "Crm",
        status = "scaffolded",
        roadmapPhase = "Phase 4 — CRM & HRMS",
    });
}
