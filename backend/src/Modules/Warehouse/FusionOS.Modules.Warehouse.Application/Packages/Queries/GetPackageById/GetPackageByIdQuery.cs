using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Warehouse.Application.Packages.Contracts;

namespace FusionOS.Modules.Warehouse.Application.Packages.Queries.GetPackageById;

public sealed record GetPackageByIdQuery(Guid CompanyId, Guid Id)
    : IQuery<PackageDto?>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "warehouse.package.read" };
}
