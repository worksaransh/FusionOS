using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Sales.Application.Customers.Contracts;

namespace FusionOS.Modules.Sales.Application.Customers.Commands.UpdateCustomer;

public sealed record UpdateCustomerCommand(Guid CompanyId, Guid CustomerId, string Name, string? ContactEmail, decimal CreditLimit)
    : ICommand<CustomerDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "sales.customer.update" };
    public string EntityType => nameof(Domain.Customers.Customer);
    public Guid EntityId => CustomerId;
    public string Action => "Updated";
}
