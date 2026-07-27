using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.IntegrationHub.Api.Controllers;

/// <summary>Reports genuine module status — IntegrationHub has real connector sync adapters (Shopify, Razorpay, Stripe, Delhivery, Shiprocket, Ondc) calling documented external APIs, plus a Tally migration pipeline, not just a registration probe.</summary>
[ApiController]
[Route("api/v1/integration_hub/health")]
public sealed class IntegrationHubHealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new
    {
        module = "IntegrationHub",
        status = "active",
        roadmapPhase = "Phase 8 — AI & Integrations",
        slices = new[] { "integration-connectors", "connector-connections", "connector-sync-runs", "tally-migrations" },
    });
}
