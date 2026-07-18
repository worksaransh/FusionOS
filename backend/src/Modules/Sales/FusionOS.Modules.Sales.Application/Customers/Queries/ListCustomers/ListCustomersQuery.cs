using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Sales.Application.Customers.Contracts;

namespace FusionOS.Modules.Sales.Application.Customers.Queries.ListCustomers;

/// <summary>Read-gated (2026-07-14 sprint audit) — see ListAccountsQuery for rationale.</summary>
public sealed record ListCustomersQuery(Guid CompanyId, string? Search = null, int Page = 1, int PageSize = 25)
    : IQuery<PagedResult<CustomerDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "sales.customer.read" };
}
