using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Sales.Application.Quotations.Contracts;
using FusionOS.Modules.Sales.Domain.Quotations;

namespace FusionOS.Modules.Sales.Application.Quotations.Commands.CreateQuotation;

public sealed record CreateQuotationCommand(Guid CompanyId, Guid CustomerId, IReadOnlyList<QuotationLineInput> Lines)
    : ICommand<QuotationDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "sales.quotation.create" };
    public string EntityType => nameof(Domain.Quotations.Quotation);
    public Guid EntityId { get; init; }
    public string Action => "Created";
}
