using FusionOS.SharedKernel;

namespace FusionOS.Modules.Warehouse.Domain.GoodsReceipts.Events;

/// <summary>
/// Raised when a GoodsReceiptLine's putaway is confirmed (docs/IMPLEMENTATION_PLAN.md
/// item 12: "Putaway — a suggested/confirmed putaway location on Goods Receipt").
/// Has **no consumer this phase** — same deliberate restraint as PickListPacked
/// (Phase M9's Picking+Packing slice): it is the natural future hook for a
/// "current stock by bin" read model, which does not exist yet anywhere in this
/// codebase (Bin is purely a location entity today, with no quantity tracking of
/// its own). Wiring a consumer for a read model that isn't built yet would be
/// worse than leaving this documented and unwired, same reasoning as every other
/// deliberately-orphaned event in docs/ORPHANED_EVENTS_AUDIT.md.
/// </summary>
public sealed record GoodsReceiptLinePutAway(
    Guid GoodsReceiptId,
    Guid CompanyId,
    Guid LineId,
    Guid ProductId,
    Guid BinId) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
