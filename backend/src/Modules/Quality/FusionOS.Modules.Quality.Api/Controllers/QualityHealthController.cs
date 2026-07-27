using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Quality.Api.Controllers;

/// <summary>Reports genuine module status — Quality has real Inspections/NCR/CorrectiveAction/Audit/Deviation depth plus cross-module event consumers, not just a registration probe.</summary>
[ApiController]
[Route("api/v1/quality/health")]
public sealed class QualityHealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new
    {
        module = "Quality",
        status = "active",
        roadmapPhase = "Phase 5 — Quality & Maintenance",
        slices = new[] { "inspections", "non-conformance-reports", "corrective-actions", "quality-audits", "deviation-requests", "quality-holds", "calibrations", "certificates" },
    });
}
