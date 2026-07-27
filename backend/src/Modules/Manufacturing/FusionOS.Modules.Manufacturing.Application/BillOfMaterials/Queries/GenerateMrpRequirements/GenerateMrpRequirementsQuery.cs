using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Contracts;

namespace FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Queries.GenerateMrpRequirements;

/// <summary>
/// Material Requirements Planning — GROSS requirements only (Phase 6, 2026-07-23).
///
/// Given a company-wide demand list (one or more <see cref="MrpDemandLine"/>s — "we need
/// this much of this finished/manufactured product"), looks up each product's primary
/// BillOfMaterials and reuses the exact same recursive explosion algorithm as
/// ExplodeBillOfMaterialsQuery (<see cref="Contracts.BomExplosionEngine"/> — not
/// duplicated here) to flatten every demand line down to its leaf component
/// requirements, then sums all of them into one company-wide total per product.
///
/// <b>Investigated (2026-07-23) and deliberately scoped down:</b> a real MRP run nets
/// these gross requirements against what's already on hand or already inbound — on-hand
/// Inventory stock levels, open/scheduled Purchase Orders, and in-process Work Orders —
/// to produce actual "go buy/make this much more" NET requirements, typically also
/// time-phased against lead times. None of that is data this module can see: Manufacturing
/// has no cross-module query or FK into Inventory's stock ledger, Procurement's purchase
/// orders, or even its own Work Orders' remaining-to-produce quantities here (03_SYSTEM_
/// ARCHITECTURE.md §2 — ComponentProductId/ProductId are opaque references, same reasoning
/// BillOfMaterials.cs and GetTaxPeriodSummaryReportQuery's own doc comment already document
/// for their respective modules). So "how much do we already have or already have coming"
/// is simply not answerable from in here today.
///
/// Extending this into real net requirements would need either a read-only cross-module
/// query into Inventory (a deliberate, reviewed exception to the module-boundary rule) or
/// an integration-event-fed local read model of stock levels — a real, named follow-up,
/// not something this slice reaches into. Until then, this is the honest, correctly-shaped
/// gross-requirements scaffold: it tells a planner exactly how much of each leaf component
/// the requested production would consume, nothing more.
///
/// Gated by the new manufacturing.mrp.run permission (distinct from
/// manufacturing.bill-of-materials.read: running MRP is a planning action across
/// potentially many BOMs/products, not simply viewing one BOM).
/// </summary>
public sealed record GenerateMrpRequirementsQuery(Guid CompanyId, IReadOnlyList<MrpDemandLine> DemandLines)
    : IQuery<MrpRequirementsResultDto>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "manufacturing.mrp.run" };
}
