using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Procurement.Api.Controllers;

/// <summary>Reports genuine module status — Procurement has grown well past its Phase 1 core into Rfqs/SupplierQuotes/SupplierContracts/VendorReturns/SupplierRatings depth.</summary>
[ApiController]
[Route("api/v1/procurement/health")]
public sealed class ProcurementHealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new
    {
        module = "Procurement",
        status = "active",
        roadmapPhase = "Phase 1 — Trading ERP Core",
        slices = new[] { "suppliers", "rfqs", "supplier-quotes", "purchase-orders", "supplier-contracts", "vendor-returns", "supplier-ratings" },
    });
}
