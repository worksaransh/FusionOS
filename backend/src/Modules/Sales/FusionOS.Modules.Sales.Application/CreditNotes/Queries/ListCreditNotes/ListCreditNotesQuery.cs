using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Sales.Application.CreditNotes.Contracts;

namespace FusionOS.Modules.Sales.Application.CreditNotes.Queries.ListCreditNotes;

/// <summary>Read-gated, same convention as ListInvoicesQuery — see ListAccountsQuery for rationale.</summary>
public sealed record ListCreditNotesQuery(Guid CompanyId, int Page = 1, int PageSize = 25)
    : IQuery<PagedResult<CreditNoteDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "sales.credit-note.read" };
}
