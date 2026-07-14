using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Sales.Application.SalesOrders.Contracts;
using FusionOS.Modules.Sales.Domain.SalesOrders;

namespace FusionOS.Modules.Sales.Application.SalesOrders.Commands.CreateSalesOrder;

public sealed record CreateSalesOrderCommand(Guid CompanyId, Guid CustomerId, IReadOnlyList<SalesOrderLineInput> Lines)
    : ICommand<SalesOrderDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "sales.sales-order.create" };
    public string EntityType => nameof(Domain.SalesOrders.SalesOrder);
    public Guid EntityId { get; init; }
    public string Action => "Created";
}
