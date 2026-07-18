using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Sales.Application.Quotations.Contracts;

namespace FusionOS.Modules.Sales.Application.Quotations.Commands.AcceptQuotation;

public sealed record AcceptQuotationCommand(Guid CompanyId, Guid QuotationId)
    : ICommand<QuotationDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "sales.quotation.accept" };
    public string EntityType => nameof(Domain.Quotations.Quotation);
    public Guid EntityId => QuotationId;
    public string Action => "Accepted";
}
