using FusionOS.SharedKernel;

namespace FusionOS.Modules.Procurement.Domain.Rfqs.Events;

/// <summary>
/// Raised once an Rfq has been converted into a real PurchaseOrder. Has **no
/// consumer this phase** — the natural future hook for a supplier-scorecard or
/// RFQ-win-rate report that doesn't exist yet anywhere in this codebase, same
/// deliberate restraint as QuotationConverted/PickListPacked's unwired state.
/// </summary>
public sealed record RfqConverted(Guid RfqId, Guid CompanyId, Guid SupplierId, Guid PurchaseOrderId) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
