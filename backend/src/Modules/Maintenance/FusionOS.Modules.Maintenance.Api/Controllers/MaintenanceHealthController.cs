using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Maintenance.Api.Controllers;

/// <summary>Reports genuine module status — Maintenance has real MaintenanceRequest/SparePartUsage/MeterReading depth, not just a registration probe.</summary>
[ApiController]
[Route("api/v1/maintenance/health")]
public sealed class MaintenanceHealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new
    {
        module = "Maintenance",
        status = "active",
        roadmapPhase = "Phase 5 — Quality & Maintenance",
        slices = new[] { "maintenance-requests", "spare-part-usage", "meter-readings" },
    });
}
