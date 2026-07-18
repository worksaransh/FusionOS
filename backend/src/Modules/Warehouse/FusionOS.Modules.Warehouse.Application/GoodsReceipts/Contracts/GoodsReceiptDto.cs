namespace FusionOS.Modules.Warehouse.Application.GoodsReceipts.Contracts;

public sealed record GoodsReceiptLineDto(
    Guid Id,
    Guid ProductId,
    decimal QuantityReceived,
    decimal? UnitCost,
    string? BatchNumber,
    string? SerialNumber,
    Guid? SuggestedBinId,
    Guid? PutAwayBinId,
    bool IsPutAway);

public sealed record GoodsReceiptDto(
    Guid Id,
    Guid WarehouseId,
    Guid ZoneId,
    Guid? PurchaseOrderId,
    Guid? SupplierId,
    DateTimeOffset ReceivedDate,
    IReadOnlyList<GoodsReceiptLineDto> Lines);
