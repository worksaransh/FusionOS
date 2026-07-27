using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Inventory.Api.Controllers;

/// <summary>Reports genuine module status — Inventory has grown well past its Phase 1 core into Batches/SerialUnits/Attributes/Transfers/Reservations/FIFO-and-weighted-average costing depth.</summary>
[ApiController]
[Route("api/v1/inventory/health")]
public sealed class InventoryHealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new
    {
        module = "Inventory",
        status = "active",
        roadmapPhase = "Phase 1 — Trading ERP Core",
        slices = new[] { "products", "batches", "serial-units", "attributes", "transfers", "reservations", "costing", "ledger" },
    });
}
