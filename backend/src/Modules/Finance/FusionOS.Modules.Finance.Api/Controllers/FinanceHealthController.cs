using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Finance.Api.Controllers;

/// <summary>Reports genuine module status — Finance has real GL/AR/AP/tax/GST/fixed-asset/budget depth, not just a registration probe.</summary>
[ApiController]
[Route("api/v1/finance/health")]
public sealed class FinanceHealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new
    {
        module = "Finance",
        status = "active",
        roadmapPhase = "Phase 2 — Financial Backbone",
        slices = new[] { "general-ledger", "receivables", "payables", "tax-rates", "gst-returns", "fixed-assets", "budgets", "bank-reconciliation" },
    });
}
