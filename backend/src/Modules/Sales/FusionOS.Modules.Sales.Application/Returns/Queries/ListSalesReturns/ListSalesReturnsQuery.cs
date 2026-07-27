using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Sales.Application.Returns.Contracts;

namespace FusionOS.Modules.Sales.Application.Returns.Queries.ListSalesReturns;

/// <summary>Read-gated, same convention as ListCreditNotesQuery — see ListAccountsQuery for rationale. Status is an optional exact-match filter (e.g. "Requested", "Approved").</summary>
public sealed record ListSalesReturnsQuery(Guid CompanyId, int Page = 1, int PageSize = 25, string? Status = null)
    : IQuery<PagedResult<SalesReturnDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "sales.return.read" };
}
