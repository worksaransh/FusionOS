using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Sales.Application.SalesOrders.Contracts;

namespace FusionOS.Modules.Sales.Application.SalesOrders.Commands.ConfirmSalesOrder;

public sealed record ConfirmSalesOrderCommand(Guid CompanyId, Guid SalesOrderId)
    : ICommand<SalesOrderDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "sales.sales-order.confirm" };
    public string EntityType => nameof(Domain.SalesOrders.SalesOrder);
    public Guid EntityId => SalesOrderId;
    public string Action => "Confirmed";
}
