using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Sales.Application.SalesOrders.Contracts;

namespace FusionOS.Modules.Sales.Application.SalesOrders.Queries.ListSalesOrders;

public sealed record ListSalesOrdersQuery(Guid CompanyId, int Page = 1, int PageSize = 25) : IQuery<PagedResult<SalesOrderDto>>;
