using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Core.Application.FeatureFlags.Contracts;

namespace FusionOS.Modules.Core.Application.FeatureFlags.Queries.GetFeatureFlagById;

public sealed record GetFeatureFlagByIdQuery(Guid CompanyId, Guid FeatureFlagId) : IQuery<FeatureFlagDto>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "core.feature-flag.read" };
}
