using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Warehouse.Application.Bins.Contracts;

namespace FusionOS.Modules.Warehouse.Application.Bins.Queries.GetBinById;

public sealed record GetBinByIdQuery(Guid CompanyId, Guid Id)
    : IQuery<BinDto?>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "warehouse.bin.read" };
}
