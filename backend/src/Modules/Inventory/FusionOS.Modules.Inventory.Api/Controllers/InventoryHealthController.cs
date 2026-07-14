using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Inventory.Api.Controllers;

[ApiController]
[Route("api/v1/inventory/health")]
public sealed class InventoryHealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new { module = "Inventory", status = "phase-1-in-progress", roadmapPhase = "Phase 1 — Trading ERP Core" });
}
