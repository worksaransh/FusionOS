using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Sales.Application.Dispatches.Contracts;

namespace FusionOS.Modules.Sales.Application.Dispatches.Queries.ListDispatches;

public sealed record ListDispatchesQuery(Guid CompanyId, int Page = 1, int PageSize = 25) : IQuery<PagedResult<DispatchDto>>;
