namespace FusionOS.Modules.Procurement.Domain.PurchaseOrders;

/// <summary>Input shape for creating a PurchaseOrder — kept in Domain so both Application and tests can reference one definition.</summary>
public sealed record PurchaseOrderLineInput(Guid ProductId, decimal Quantity, decimal UnitPrice, Guid? TaxRateId = null, decimal TaxAmount = 0m);
