using FusionOS.SharedKernel;
using FusionOS.Modules.Procurement.Domain.SupplierRatings.Events;

namespace FusionOS.Modules.Procurement.Domain.SupplierRatings;

/// <summary>
/// A manually-recorded supplier performance rating — a genuine judgment a
/// procurement person enters (typically after a Purchase Order is fully
/// received), not a fabricated automatic metric. This is deliberately separate
/// from GetSupplierScorecardReport (Reports/Contracts/SupplierScorecardReportDto),
/// which is purely derived from PurchaseOrder history and has no on-time-delivery
/// or quality dimension because no such input exists there. SupplierRating is
/// that input: QualityScore/DeliveryScore are 1-5 human judgments, optionally
/// tied to a specific PurchaseOrderId (validated by the command handler, not
/// here, to belong to the same Supplier and be FullyReceived — rating a PO's
/// delivery performance only makes sense once it actually delivered) or left
/// null for a general periodic rating with no PO reference.
/// </summary>
public sealed class SupplierRating : TenantAggregateRoot
{
    public const int MinScore = 1;
    public const int MaxScore = 5;

    public Guid SupplierId { get; private set; }
    public Guid? PurchaseOrderId { get; private set; }
    public int QualityScore { get; private set; }
    public int DeliveryScore { get; private set; }
    public string? Comments { get; private set; }
    public Guid RatedByUserId { get; private set; }
    public DateTimeOffset RatedAt { get; private set; }

    private SupplierRating() { }

    public static SupplierRating Create(
        Guid companyId,
        Guid supplierId,
        Guid? purchaseOrderId,
        int qualityScore,
        int deliveryScore,
        Guid ratedByUserId,
        string? comments = null)
    {
        if (supplierId == Guid.Empty)
            throw new ArgumentException("Supplier id is required.", nameof(supplierId));
        if (ratedByUserId == Guid.Empty)
            throw new ArgumentException("Rated-by user id is required.", nameof(ratedByUserId));
        if (qualityScore < MinScore || qualityScore > MaxScore)
            throw new ArgumentException($"Quality score must be between {MinScore} and {MaxScore}.", nameof(qualityScore));
        if (deliveryScore < MinScore || deliveryScore > MaxScore)
            throw new ArgumentException($"Delivery score must be between {MinScore} and {MaxScore}.", nameof(deliveryScore));

        var rating = new SupplierRating
        {
            CompanyId = companyId,
            SupplierId = supplierId,
            PurchaseOrderId = purchaseOrderId,
            QualityScore = qualityScore,
            DeliveryScore = deliveryScore,
            Comments = string.IsNullOrWhiteSpace(comments) ? null : comments.Trim(),
            RatedByUserId = ratedByUserId,
            RatedAt = DateTimeOffset.UtcNow,
        };

        rating.Raise(new SupplierRatingRecorded(rating.Id, companyId, supplierId, purchaseOrderId));
        return rating;
    }
}
