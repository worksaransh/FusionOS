using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Sales.Application.Returns.Contracts;

namespace FusionOS.Modules.Sales.Application.Returns.Queries.ListSalesReturnsByInvoice;

public sealed record ListSalesReturnsByInvoiceQuery(Guid CompanyId, Guid InvoiceId) : IQuery<IReadOnlyList<SalesReturnDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "sales.return.read" };
}
