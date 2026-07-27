using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Sales.Application.Dispatches.Contracts;

namespace FusionOS.Modules.Sales.Application.Dispatches.Queries.ListMyDispatches;

/// <summary>
/// Portal counterpart of ListDispatchesQuery - see ListMyQuotationsQuery for the full reasoning on scoping/authorization.
/// Implements IPubliclyAccessible (Phase 9 hardening fix, 2026-07-25): gated by the customer-portal
/// JWT/policy rather than staff RBAC — exempt from AuthorizationBehavior's fail-closed check.
/// </summary>
public sealed record ListMyDispatchesQuery(Guid CustomerId, int Page = 1, int PageSize = 25) : IQuery<PagedResult<DispatchDto>>, IPubliclyAccessible;
