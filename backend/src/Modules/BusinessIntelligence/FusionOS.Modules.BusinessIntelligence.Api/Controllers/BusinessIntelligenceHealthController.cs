using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.BusinessIntelligence.Api.Controllers;

/// <summary>Reports genuine module status — BusinessIntelligence has real KpiDefinition/KpiSnapshot CRUD, but snapshots are still manually recorded (RecordKpiSnapshotCommand), not computed from a dashboard/aggregation engine — see KpiSnapshot's own doc comment for that scope decision.</summary>
[ApiController]
[Route("api/v1/bi/health")]
public sealed class BusinessIntelligenceHealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new
    {
        module = "BusinessIntelligence",
        status = "active-manual-kpi-tracking",
        roadmapPhase = "Phase 6 — Business Intelligence",
        slices = new[] { "kpi-definitions", "kpi-snapshots (manually recorded, no computed dashboard engine yet)" },
    });
}
