using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Sales.Application.Customers.Contracts;

namespace FusionOS.Modules.Sales.Application.Customers.Queries.ListCustomers;

public sealed record ListCustomersQuery(Guid CompanyId, string? Search = null, int Page = 1, int PageSize = 25) : IQuery<PagedResult<CustomerDto>>;
