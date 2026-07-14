using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Sales.Application.Invoices.Contracts;

namespace FusionOS.Modules.Sales.Application.Invoices.Queries.ListInvoices;

public sealed record ListInvoicesQuery(Guid CompanyId, int Page = 1, int PageSize = 25) : IQuery<PagedResult<InvoiceDto>>;
