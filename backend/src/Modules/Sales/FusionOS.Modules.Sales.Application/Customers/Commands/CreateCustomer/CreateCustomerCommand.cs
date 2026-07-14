using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Sales.Application.Customers.Contracts;

namespace FusionOS.Modules.Sales.Application.Customers.Commands.CreateCustomer;

public sealed record CreateCustomerCommand(Guid CompanyId, string Name, string Code, string? ContactEmail, decimal CreditLimit)
    : ICommand<CustomerDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "sales.customer.create" };
    public string EntityType => nameof(Domain.Customers.Customer);
    public Guid EntityId { get; init; }
    public string Action => "Created";
}
