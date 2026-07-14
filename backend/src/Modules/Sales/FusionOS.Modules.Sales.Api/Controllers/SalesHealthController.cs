using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Sales.Api.Controllers;

[ApiController]
[Route("api/v1/sales/health")]
public sealed class SalesHealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new { module = "Sales", status = "phase-1-in-progress", roadmapPhase = "Phase 1 — Trading ERP Core" });
}
