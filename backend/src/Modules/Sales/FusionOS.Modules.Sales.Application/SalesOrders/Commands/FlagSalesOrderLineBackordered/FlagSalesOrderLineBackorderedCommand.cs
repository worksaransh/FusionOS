using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Sales.Application.SalesOrders.Contracts;

namespace FusionOS.Modules.Sales.Application.SalesOrders.Commands.FlagSalesOrderLineBackordered;

public sealed record FlagSalesOrderLineBackorderedCommand(Guid CompanyId, Guid SalesOrderId, Guid LineId, decimal BackorderedQuantity)
    : ICommand<SalesOrderDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "sales.sales-order.flag-backorder" };
    public string EntityType => nameof(Domain.SalesOrders.SalesOrder);
    public Guid EntityId => SalesOrderId;
    public string Action => "LineBackordered";
}
