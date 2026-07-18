using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Sales.Application.Customers.Contracts;

namespace FusionOS.Modules.Sales.Application.Customers.Queries.GetCustomerById;

/// <summary>Tenant-scoped single-record read, gated the same as ListCustomersQuery.</summary>
public sealed record GetCustomerByIdQuery(Guid CompanyId, Guid CustomerId) : IQuery<CustomerDto>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "sales.customer.read" };
}
