using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Sales.Application.Quotations.Contracts;

namespace FusionOS.Modules.Sales.Application.Quotations.Queries.ListQuotations;

/// <summary>Read-gated, same convention as ListInvoicesQuery/ListCreditNotesQuery — see ListAccountsQuery for rationale.</summary>
public sealed record ListQuotationsQuery(Guid CompanyId, int Page = 1, int PageSize = 25)
    : IQuery<PagedResult<QuotationDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "sales.quotation.read" };
}
