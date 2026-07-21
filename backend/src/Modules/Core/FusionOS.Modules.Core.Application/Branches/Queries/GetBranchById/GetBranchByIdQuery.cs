using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Core.Application.Branches.Contracts;

namespace FusionOS.Modules.Core.Application.Branches.Queries.GetBranchById;

public sealed record GetBranchByIdQuery(Guid CompanyId, Guid BranchId)
    : IQuery<BranchDto>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "core.branch.read" };
}
