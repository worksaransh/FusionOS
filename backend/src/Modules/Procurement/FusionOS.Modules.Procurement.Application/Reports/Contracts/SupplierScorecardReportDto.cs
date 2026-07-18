namespace FusionOS.Modules.Procurement.Application.Reports.Contracts;

/// <summary>
/// Per-supplier scorecard (docs/IMPLEMENTATION_PLAN.md Phase 10 item 2) computed
/// entirely from existing PurchaseOrder data — no new aggregate, no new fields.
/// Deliberately does NOT attempt an on-time-delivery rate: PurchaseOrder has no
/// expected/promised delivery date field today, and inventing one just to fake
/// this metric would be worse than omitting it. FullyReceivedRate is the
/// closest honest proxy available — how much of what was ordered from this
/// supplier actually arrived complete.
/// </summary>
public sealed record SupplierScorecardLineDto(
    Guid SupplierId,
    int OrderCount,
    decimal TotalOrderValue,
    decimal AverageOrderValue,
    int FullyReceivedCount,
    decimal FullyReceivedRate);
