using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Sales.Application.Customers.Contracts;

namespace FusionOS.Modules.Sales.Application.Customers.Commands.DeactivateCustomer;

/// <summary>Soft-deactivate only — see Customer.Deactivate(). Never a hard delete.</summary>
public sealed record DeactivateCustomerCommand(Guid CompanyId, Guid CustomerId)
    : ICommand<CustomerDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "sales.customer.deactivate" };
    public string EntityType => nameof(Domain.Customers.Customer);
    public Guid EntityId => CustomerId;
    public string Action => "Deactivated";
}
