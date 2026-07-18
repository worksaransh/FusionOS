using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Sales.Application.SalesOrders.Contracts;

namespace FusionOS.Modules.Sales.Application.Quotations.Commands.ConvertQuotationToSalesOrder;

/// <summary>
/// Converts an Accepted Quotation into a real SalesOrder — the point of the
/// whole aggregate (docs/IMPLEMENTATION_PLAN.md Phase 10 item 8). Returns the
/// newly created SalesOrderDto, not a QuotationDto, since that's the artifact
/// the caller actually wants out of a conversion.
/// </summary>
public sealed record ConvertQuotationToSalesOrderCommand(Guid CompanyId, Guid QuotationId)
    : ICommand<SalesOrderDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "sales.quotation.convert" };
    public string EntityType => nameof(Domain.Quotations.Quotation);
    public Guid EntityId => QuotationId;
    public string Action => "Converted";
}
