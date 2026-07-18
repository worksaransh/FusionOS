using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Sales.Application.Invoices.Contracts;

namespace FusionOS.Modules.Sales.Application.Invoices.Queries.ListInvoices;

/// <summary>Read-gated (2026-07-14 sprint audit) — see ListAccountsQuery for rationale.</summary>
public sealed record ListInvoicesQuery(Guid CompanyId, int Page = 1, int PageSize = 25)
    : IQuery<PagedResult<InvoiceDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "sales.invoice.read" };
}
