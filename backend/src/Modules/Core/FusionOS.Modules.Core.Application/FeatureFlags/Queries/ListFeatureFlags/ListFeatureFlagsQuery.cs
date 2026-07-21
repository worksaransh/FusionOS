using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Core.Application.FeatureFlags.Contracts;

namespace FusionOS.Modules.Core.Application.FeatureFlags.Queries.ListFeatureFlags;

public sealed record ListFeatureFlagsQuery(Guid CompanyId, string? Search = null, int Page = 1, int PageSize = 25)
    : IQuery<PagedResult<FeatureFlagDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "core.feature-flag.read" };
}
