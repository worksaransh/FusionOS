using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Warehouse.Application.Packages.Contracts;

namespace FusionOS.Modules.Warehouse.Application.Packages.Queries.ListPackagesByPickList;

public sealed record ListPackagesByPickListQuery(Guid CompanyId, Guid PickListId, int Page = 1, int PageSize = 25)
    : IQuery<PagedResult<PackageDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "warehouse.package.read" };
}
