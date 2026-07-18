using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Sales.Application.Customers.Contracts;

namespace FusionOS.Modules.Sales.Application.Customers.Commands.AssignPriceList;

public sealed record AssignPriceListCommand(Guid CompanyId, Guid CustomerId, Guid? PriceListId)
    : ICommand<CustomerDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "sales.customer.assign-price-list" };
    public string EntityType => nameof(Domain.Customers.Customer);
    public Guid EntityId => CustomerId;
    public string Action => "PriceListAssigned";
}
