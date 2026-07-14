using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Warehouse.Api.Controllers;

[ApiController]
[Route("api/v1/warehouse/health")]
public sealed class WarehouseHealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new { module = "Warehouse", status = "phase-1-in-progress", roadmapPhase = "Phase 1 — Trading ERP Core" });
}
