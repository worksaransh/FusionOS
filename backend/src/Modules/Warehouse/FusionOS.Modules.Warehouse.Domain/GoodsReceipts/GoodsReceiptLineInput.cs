namespace FusionOS.Modules.Warehouse.Domain.GoodsReceipts;

/// <summary>Input shape for a single received line when creating a GoodsReceipt (mirrors PurchaseOrderLineInput's role in Procurement).</summary>
public sealed record GoodsReceiptLineInput(Guid ProductId, decimal QuantityReceived, decimal? UnitCost);
