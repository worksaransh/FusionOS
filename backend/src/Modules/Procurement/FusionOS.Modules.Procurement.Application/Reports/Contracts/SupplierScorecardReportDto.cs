namespace FusionOS.Modules.Procurement.Application.Reports.Contracts;

/// <summary>
/// Per-supplier scorecard (docs/IMPLEMENTATION_PLAN.md Phase 10 item 2) computed
/// entirely from existing PurchaseOrder data — no new aggregate, no new fields.
/// Deliberately does NOT attempt an on-time-delivery rate: PurchaseOrder has no
/// expected/promised delivery date field today, and inventing one just to fake
/// this metric would be worse than omitting it. FullyReceivedRate is the
/// closest honest proxy available — how much of what was ordered from this
/// supplier actually arrived complete.
///
/// AverageQualityScore/AverageDeliveryScore/RatingCount (added alongside the
/// SupplierRating aggregate) are a genuinely different kind of figure from the
/// fields above: a human-entered judgment, not something derived from PO
/// history. They stay nullable and are never defaulted to 0 — null means "no
/// ratings recorded yet for this supplier", which is a materially different
/// fact from "rated a 0", so faking a number here would be exactly the kind of
/// dishonest metric this DTO's original on-time-delivery omission was written
/// to avoid.
/// </summary>
public sealed record SupplierScorecardLineDto(
    Guid SupplierId,
    int OrderCount,
    decimal TotalOrderValue,
    decimal AverageOrderValue,
    int FullyReceivedCount,
    decimal FullyReceivedRate,
    double? AverageQualityScore = null,
    double? AverageDeliveryScore = null,
    int? RatingCount = null);
