using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Marketplace.Api.Controllers;

/// <summary>Reports genuine module status — Marketplace has real PluginListing catalog and PluginInstallation lifecycle (install/enable/disable/uninstall) bookkeeping, but deliberately no plugin execution runtime — see PluginInstallation's own doc comment for that scope decision.</summary>
[ApiController]
[Route("api/v1/marketplace/health")]
public sealed class MarketplaceHealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new
    {
        module = "Marketplace",
        status = "active-catalog-and-bookkeeping",
        roadmapPhase = "Phase 8 — Marketplace & Ecosystem",
        slices = new[] { "plugin-listings", "plugin-installations (bookkeeping only, no execution runtime)" },
    });
}
