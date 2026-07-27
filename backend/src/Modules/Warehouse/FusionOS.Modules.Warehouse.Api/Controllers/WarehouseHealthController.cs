using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Warehouse.Api.Controllers;

/// <summary>Reports genuine module status — Warehouse has grown well past its Phase 1 core into Zones/Racks/Shelves/Bins/PickLists/GoodsReceipts/CycleCounts/Packages depth.</summary>
[ApiController]
[Route("api/v1/warehouse/health")]
public sealed class WarehouseHealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new
    {
        module = "Warehouse",
        status = "active",
        roadmapPhase = "Phase 1 — Trading ERP Core",
        slices = new[] { "warehouses", "zones", "racks-shelves-bins", "goods-receipts", "pick-lists", "cycle-counts", "packages" },
    });
}
