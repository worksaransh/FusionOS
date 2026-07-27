using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Sales.Api.Controllers;

/// <summary>Reports genuine module status — Sales has grown well past its Phase 1 core into Quotations/Invoices/Dispatches/Returns/CreditNotes/PortalUsers depth.</summary>
[ApiController]
[Route("api/v1/sales/health")]
public sealed class SalesHealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new
    {
        module = "Sales",
        status = "active",
        roadmapPhase = "Phase 1 — Trading ERP Core",
        slices = new[] { "customers", "price-lists", "quotations", "sales-orders", "invoices", "dispatches", "returns", "credit-notes", "portal-users" },
    });
}
