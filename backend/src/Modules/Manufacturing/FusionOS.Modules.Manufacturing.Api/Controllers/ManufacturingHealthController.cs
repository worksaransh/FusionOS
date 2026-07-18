using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Manufacturing.Api.Controllers;

/// <summary>The only endpoint this not-yet-implemented module exposes: proof it is registered and reachable.</summary>
[ApiController]
[Route("api/v1/manufacturing/health")]
public sealed class ManufacturingHealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new
    {
        module = "Manufacturing",
        status = "active",
        roadmapPhase = "Phase 3 — Manufacturing ERP",
        slices = new[] { "bills-of-materials", "work-orders" },
    });
}
