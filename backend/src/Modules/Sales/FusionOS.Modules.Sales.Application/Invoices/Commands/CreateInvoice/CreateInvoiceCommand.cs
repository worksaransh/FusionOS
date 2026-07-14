using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Sales.Application.Invoices.Contracts;
using FusionOS.Modules.Sales.Domain.Invoices;

namespace FusionOS.Modules.Sales.Application.Invoices.Commands.CreateInvoice;

public sealed record CreateInvoiceCommand(Guid CompanyId, Guid SalesOrderId, Guid CustomerId, IReadOnlyList<InvoiceLineInput> Lines)
    : ICommand<InvoiceDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "sales.invoice.create" };
    public string EntityType => nameof(Domain.Invoices.Invoice);
    public Guid EntityId { get; init; }
    public string Action => "Created";
}
