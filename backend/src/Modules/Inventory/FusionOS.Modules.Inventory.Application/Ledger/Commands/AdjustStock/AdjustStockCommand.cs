using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Inventory.Application.Ledger.Contracts;

namespace FusionOS.Modules.Inventory.Application.Ledger.Commands.AdjustStock;

/// <summary>Covers the PRD's "Stock Adjustment" capability — the first ledger-writing operation. Receipts/issues driven by Procurement/Sales/Manufacturing events are a later slice.</summary>
public sealed record AdjustStockCommand(Guid CompanyId, Guid ProductId, Guid WarehouseId, decimal QuantityDelta, string Reason, decimal? UnitCost)
    : ICommand<InventoryLedgerEntryDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "inventory.stock.adjust" };
    public string EntityType => nameof(Domain.Ledger.InventoryLedgerEntry);
    public Guid EntityId { get; init; }
    public string Action => "StockAdjusted";
}
