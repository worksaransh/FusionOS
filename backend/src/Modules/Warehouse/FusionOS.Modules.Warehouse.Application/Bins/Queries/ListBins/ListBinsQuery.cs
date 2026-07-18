using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Warehouse.Application.Bins.Contracts;

namespace FusionOS.Modules.Warehouse.Application.Bins.Queries.ListBins;

public sealed record ListBinsQuery(Guid CompanyId, Guid ZoneId, int Page = 1, int PageSize = 25)
    : IQuery<PagedResult<BinDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "warehouse.bin.read" };
}
