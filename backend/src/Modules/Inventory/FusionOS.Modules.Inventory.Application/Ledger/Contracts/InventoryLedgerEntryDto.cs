namespace FusionOS.Modules.Inventory.Application.Ledger.Contracts;

public sealed record InventoryLedgerEntryDto(Guid Id, Guid ProductId, Guid WarehouseId, decimal QuantityDelta, decimal? UnitCost, string? BatchNumber, string? SerialNumber, string Reason, DateTimeOffset TransactionDate);

public sealed record StockOnHandDto(Guid ProductId, Guid? WarehouseId, decimal QuantityOnHand);
