namespace FusionOS.Modules.Procurement.Application.SupplierRatings.Contracts;

public sealed record SupplierRatingDto(
    Guid Id,
    Guid SupplierId,
    Guid? PurchaseOrderId,
    int QualityScore,
    int DeliveryScore,
    string? Comments,
    Guid RatedByUserId,
    DateTimeOffset RatedAt);
