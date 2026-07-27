using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Sales.Application.SalesOrders.Contracts;

namespace FusionOS.Modules.Sales.Application.SalesOrders.Queries.ListMySalesOrders;

/// <summary>
/// Portal counterpart of ListSalesOrdersQuery - see ListMyQuotationsQuery for the full reasoning on scoping/authorization.
/// Implements IPubliclyAccessible (Phase 9 hardening fix, 2026-07-25): gated by the customer-portal
/// JWT/policy rather than staff RBAC — exempt from AuthorizationBehavior's fail-closed check.
/// </summary>
public sealed record ListMySalesOrdersQuery(Guid CustomerId, int Page = 1, int PageSize = 25) : IQuery<PagedResult<SalesOrderDto>>, IPubliclyAccessible;
