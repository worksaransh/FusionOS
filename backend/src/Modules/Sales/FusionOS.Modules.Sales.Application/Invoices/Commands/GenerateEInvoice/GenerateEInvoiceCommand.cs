using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Sales.Application.Invoices.Contracts;

namespace FusionOS.Modules.Sales.Application.Invoices.Commands.GenerateEInvoice;

public sealed record GenerateEInvoiceCommand(Guid CompanyId, Guid InvoiceId)
    : ICommand<InvoiceDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "sales.invoice.generate-e-invoice" };
    public string EntityType => nameof(Domain.Invoices.Invoice);
    public Guid EntityId => InvoiceId;
    public string Action => "EInvoiceGenerated";
}
