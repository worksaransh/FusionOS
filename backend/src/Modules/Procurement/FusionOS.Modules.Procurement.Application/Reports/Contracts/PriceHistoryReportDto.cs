namespace FusionOS.Modules.Procurement.Application.Reports.Contracts;

/// <summary>
/// One historical unit price paid for a Product (Phase 1 closeout, 2026-07-18) —
/// computed entirely from existing PurchaseOrder/PurchaseOrderLine data, no new
/// aggregate, no new fields. Same restraint as SupplierScorecardLineDto above.
/// </summary>
public sealed record PriceHistoryLineDto(Guid PurchaseOrderId, Guid SupplierId, DateTimeOffset OrderDate, decimal UnitPrice, decimal Quantity);
