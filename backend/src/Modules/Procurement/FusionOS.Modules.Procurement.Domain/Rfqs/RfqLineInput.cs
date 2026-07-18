namespace FusionOS.Modules.Procurement.Domain.Rfqs;

/// <summary>
/// Unlike PurchaseOrderLineInput/QuotationLineInput, an Rfq line carries no price —
/// pricing is exactly what suppliers are being asked to submit via SupplierQuote.
/// </summary>
public sealed record RfqLineInput(Guid ProductId, decimal Quantity);
