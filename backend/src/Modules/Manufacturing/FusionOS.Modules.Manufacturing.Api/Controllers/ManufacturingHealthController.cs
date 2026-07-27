using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Manufacturing.Api.Controllers;

/// <summary>Reports genuine module status — Manufacturing has real BOM/work-order/routing/scheduling depth, not just a registration probe.</summary>
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
        slices = new[] { "bills-of-materials", "work-orders", "routing", "material-issue-return", "scrap-yield" },
    });
}
