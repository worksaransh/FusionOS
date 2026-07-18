using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Sales.Application.SalesOrders.Contracts;

namespace FusionOS.Modules.Sales.Application.SalesOrders.Queries.ListSalesOrders;

/// <summary>Read-gated (2026-07-14 sprint audit) — see ListAccountsQuery for rationale.</summary>
public sealed record ListSalesOrdersQuery(Guid CompanyId, int Page = 1, int PageSize = 25)
    : IQuery<PagedResult<SalesOrderDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "sales.sales-order.read" };
}
