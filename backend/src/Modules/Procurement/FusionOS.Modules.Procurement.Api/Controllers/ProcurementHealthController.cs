using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Procurement.Api.Controllers;

[ApiController]
[Route("api/v1/procurement/health")]
public sealed class ProcurementHealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new { module = "Procurement", status = "phase-1-in-progress", roadmapPhase = "Phase 1 — Trading ERP Core" });
}
