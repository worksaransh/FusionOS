using FusionOS.SharedKernel;
using FusionOS.Modules.Warehouse.Domain.PickLists.Events;

namespace FusionOS.Modules.Warehouse.Domain.PickLists;

/// <summary>
/// Picking + Packing (Phase M9 WMS depth, 2026-07-15) — docs/IMPLEMENTATION_PLAN.md Phase 9 items
/// 10 ("Picking — a pick-list generated from a confirmed Sales Order / Dispatch, assignable to a
/// user") and 11 ("Packing — a pack confirmation step after picking, before Dispatch is marked
/// shipped"), modeled as one aggregate with a four-state lifecycle since packing is just the final
/// step of the same document, not a separate one.
///
/// SalesOrderId is an opaque reference into Sales' own SalesOrder aggregate — same
/// no-cross-module-FK convention as everywhere else (03_SYSTEM_ARCHITECTURE.md §2). This module
/// does not validate that the SalesOrderId exists or that the requested lines/quantities match the
/// sales order's own lines: doing that would require a direct compile-time reference from
/// Warehouse.Application into Sales.Application/Domain, which no other cross-module relationship in
/// this codebase takes (Inventory's ledger entries treat WarehouseId/ProductId the same opaque way
/// — see InventoryLedgerEntry's own doc comment). Callers (the frontend, which already fetches
/// Sales Orders via its own API for the picker) are responsible for supplying a real id; adding a
/// real existence/line-match check is a documented follow-up, not implemented in this slice.
///
/// BinId on a line IS validated (by the command handler, not here) — Bin lives in this same
/// Warehouse module, so that's a normal same-module reference, not a cross-module one.
///
/// WarehouseId, ProductId per line: also opaque-style references (Warehouse's own aggregate and
/// Inventory's Product respectively), consistent with GoodsReceipt's existing conventions.
/// </summary>
public sealed class PickList : TenantAggregateRoot
{
    private readonly List<PickListLine> _lines = new();

    public Guid WarehouseId { get; private set; }
    public Guid SalesOrderId { get; private set; }
    public Guid? AssignedToUserId { get; private set; }
    public PickListStatus Status { get; private set; }
    public IReadOnlyList<PickListLine> Lines => _lines.AsReadOnly();

    private PickList() { }

    public static PickList Create(Guid companyId, Guid warehouseId, Guid salesOrderId, IReadOnlyCollection<PickListLineInput> lines)
    {
        if (warehouseId == Guid.Empty)
            throw new ArgumentException("Warehouse id is required.", nameof(warehouseId));
        if (salesOrderId == Guid.Empty)
            throw new ArgumentException("Sales order id is required.", nameof(salesOrderId));
        if (lines is null || lines.Count == 0)
            throw new ArgumentException("A pick list must have at least one line.", nameof(lines));

        var pickList = new PickList
        {
            CompanyId = companyId,
            WarehouseId = warehouseId,
            SalesOrderId = salesOrderId,
            Status = PickListStatus.Pending,
        };

        foreach (var line in lines)
            pickList._lines.Add(PickListLine.Create(line.ProductId, line.BinId, line.QuantityToPick));

        pickList.Raise(new PickListCreated(pickList.Id, companyId, warehouseId, salesOrderId));
        return pickList;
    }

    /// <summary>
    /// Assigns (or reassigns) the pick list to a user. Allowed any time before Packed — a
    /// mid-picking reassignment (someone hands the list to a colleague) is a normal warehouse
    /// operation, not an error. Does not itself advance Pending -> Assigned if the list is already
    /// past that point (Picked stays Picked), it only ever moves Pending forward the first time.
    /// </summary>
    public void AssignTo(Guid assignedToUserId)
    {
        if (assignedToUserId == Guid.Empty)
            throw new ArgumentException("Assigned-to user id is required.", nameof(assignedToUserId));
        if (Status == PickListStatus.Packed)
            throw new InvalidOperationException("This pick list is already packed — it cannot be reassigned.");

        AssignedToUserId = assignedToUserId;
        if (Status == PickListStatus.Pending)
            Status = PickListStatus.Assigned;
    }

    /// <summary>
    /// Records the quantity picked so far for one line (an absolute value, not a delta — see
    /// PickListLine.RecordPicked). Requires the list to already be assigned to someone — picking
    /// without an assignee makes no sense operationally, same "enforce the real-world precondition
    /// in the domain, not just via permission" ethic used by ApprovalRequest's sequential-turn
    /// check. Once every line is fully picked, the list auto-advances to Picked — a pure
    /// bookkeeping transition, not raised as its own event (unlike Pack, which is the
    /// business-significant moment worth notifying about).
    /// </summary>
    public void RecordPick(Guid lineId, decimal quantityPicked)
    {
        if (Status == PickListStatus.Packed)
            throw new InvalidOperationException("This pick list is already packed — no further picks can be recorded.");
        if (AssignedToUserId is null)
            throw new InvalidOperationException("This pick list must be assigned to someone before picking can be recorded.");

        var line = _lines.SingleOrDefault(l => l.Id == lineId)
            ?? throw new ArgumentException($"Line '{lineId}' does not belong to this pick list.", nameof(lineId));

        line.RecordPicked(quantityPicked);

        if (Status != PickListStatus.Picked && _lines.All(l => l.IsFullyPicked))
            Status = PickListStatus.Picked;
    }

    /// <summary>Confirms packing — requires every line to already be fully picked.</summary>
    public void Pack()
    {
        if (Status != PickListStatus.Picked)
            throw new InvalidOperationException($"Only a fully-picked pick list can be packed (current status: {Status}).");

        Status = PickListStatus.Packed;
        Raise(new PickListPacked(Id, CompanyId, WarehouseId, SalesOrderId));
    }
}
