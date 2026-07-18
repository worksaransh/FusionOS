using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Inventory.Application.Ledger.Contracts;

namespace FusionOS.Modules.Inventory.Application.Ledger.Queries.GetStockOnHand;

/// <summary>Read-gated (2026-07-14 sprint audit) — see ListAccountsQuery for rationale.</summary>
public sealed record GetStockOnHandQuery(Guid CompanyId, Guid ProductId, Guid? WarehouseId)
    : IQuery<StockOnHandDto>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "inventory.stock.read" };
}
