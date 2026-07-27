using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Manufacturing.Application.WorkOrders.Contracts;

namespace FusionOS.Modules.Manufacturing.Application.WorkOrders.Queries.GetWorkInProgressSummaryReport;

/// <summary>
/// Work-in-progress summary — every Released work order is, by definition, WIP: Draft
/// hasn't started consuming material yet, and Completed/Cancelled are done (their
/// components are no longer "in progress"). For each Released order this values the
/// material currently sitting on the shop floor as
/// (component.QuantityIssued × component.UnitCost) summed across its components.
///
/// <b>UnitCost basis:</b> WorkOrderComponent.UnitCost is a snapshot of whatever the BOM
/// author supplied on BomLineInput at authoring time — it is never looked up from
/// Inventory automatically (see BomLineInput's doc comment for why: Manufacturing has
/// no query-time dependency on Inventory's costing, same module-boundary rule every
/// cross-module reference in this codebase follows). A component with no UnitCost
/// specified defaults to 0 and simply contributes nothing to this valuation — that is
/// a known, deliberate gap, not a bug, and is called out in ValuationNote below.
///
/// <b>QuantityIssued is already net of returns:</b> WorkOrderComponent.Issue adds to the
/// running QuantityIssued total and WorkOrderComponent.Return subtracts from that same
/// field (see WorkOrderComponent.cs) — there is no separate "returned" counter to
/// subtract here. QuantityIssued at any point in time already reflects material
/// genuinely still out on the floor.
///
/// This is a material-only valuation. It does not attempt to allocate labor or
/// overhead to in-flight orders — Manufacturing has no labor/overhead costing model
/// today, so there is nothing honest to report there yet.
/// </summary>
public sealed record GetWorkInProgressSummaryReportQuery(Guid CompanyId)
    : IQuery<WorkInProgressSummaryReportDto>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "manufacturing.wip.read" };
}
