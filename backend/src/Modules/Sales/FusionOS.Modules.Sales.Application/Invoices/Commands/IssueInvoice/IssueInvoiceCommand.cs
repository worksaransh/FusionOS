using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Sales.Application.Invoices.Contracts;

namespace FusionOS.Modules.Sales.Application.Invoices.Commands.IssueInvoice;

public sealed record IssueInvoiceCommand(Guid CompanyId, Guid InvoiceId)
    : ICommand<InvoiceDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "sales.invoice.issue" };
    public string EntityType => nameof(Domain.Invoices.Invoice);
    public Guid EntityId => InvoiceId;
    public string Action => "Issued";
}
