using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Sales.Application.SalesOrders.Contracts;

namespace FusionOS.Modules.Sales.Application.SalesOrders.Commands.ClearSalesOrderLineBackorder;

public sealed record ClearSalesOrderLineBackorderCommand(Guid CompanyId, Guid SalesOrderId, Guid LineId)
    : ICommand<SalesOrderDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "sales.sales-order.clear-backorder" };
    public string EntityType => nameof(Domain.SalesOrders.SalesOrder);
    public Guid EntityId => SalesOrderId;
    public string Action => "LineBackorderCleared";
}
