using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Finance.Api.Controllers;

/// <summary>The only endpoint this not-yet-implemented module exposes: proof it is registered and reachable.</summary>
[ApiController]
[Route("api/v1/finance/health")]
public sealed class FinanceHealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new
    {
        module = "Finance",
        status = "scaffolded",
        roadmapPhase = "Phase 2 — Financial Backbone",
    });
}
