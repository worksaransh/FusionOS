using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Sales.Application.Quotations.Contracts;

namespace FusionOS.Modules.Sales.Application.Quotations.Queries.ListMyQuotations;

/// <summary>
/// Portal counterpart of ListQuotationsQuery. Scoped by CustomerId (taken
/// from the caller's validated portal token - see PortalController), never a
/// caller-supplied CompanyId. Deliberately NOT gated by IRequirePermission -
/// portal authorization is the "CustomerPortal" claim policy plus this
/// CustomerId scoping, not RBAC (see ListQuotationsQuery for the staff,
/// permission-gated equivalent this must never be confused with).
/// Implements IPubliclyAccessible (Phase 10 audit fix, 2026-07-27): gated by the
/// customer-portal JWT/policy rather than staff RBAC — exempt from AuthorizationBehavior's
/// fail-closed check.
/// </summary>
public sealed record ListMyQuotationsQuery(Guid CustomerId, int Page = 1, int PageSize = 25) : IQuery<PagedResult<QuotationDto>>, IPubliclyAccessible;
