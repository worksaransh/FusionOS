using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Procurement.Application.PurchaseOrders.Contracts;

namespace FusionOS.Modules.Procurement.Application.PurchaseOrders.Queries.ListPurchaseOrders;

/// <summary>Read-gated (2026-07-14 sprint audit) — see ListAccountsQuery for rationale.</summary>
public sealed record ListPurchaseOrdersQuery(Guid CompanyId, int Page = 1, int PageSize = 25)
    : IQuery<PagedResult<PurchaseOrderDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "procurement.purchase-order.read" };
}
