using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Core.Application.Branches.Contracts;

namespace FusionOS.Modules.Core.Application.Branches.Queries.ListBranches;

public sealed record ListBranchesQuery(Guid CompanyId, string? Search = null, int Page = 1, int PageSize = 25)
    : IQuery<PagedResult<BranchDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "core.branch.read" };
}
