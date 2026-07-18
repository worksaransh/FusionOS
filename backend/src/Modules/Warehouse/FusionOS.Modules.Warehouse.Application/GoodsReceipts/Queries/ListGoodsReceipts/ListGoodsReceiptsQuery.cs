using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Warehouse.Application.GoodsReceipts.Contracts;

namespace FusionOS.Modules.Warehouse.Application.GoodsReceipts.Queries.ListGoodsReceipts;

/// <summary>Read-gated (2026-07-14 sprint audit) — see ListAccountsQuery for rationale.</summary>
public sealed record ListGoodsReceiptsQuery(Guid CompanyId, Guid WarehouseId, int Page = 1, int PageSize = 25)
    : IQuery<PagedResult<GoodsReceiptDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "warehouse.goods-receipt.read" };
}
