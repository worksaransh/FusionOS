using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Marketplace.Application.PluginInstallations.Contracts;

namespace FusionOS.Modules.Marketplace.Application.PluginInstallations.Queries.ListPluginInstallations;

public sealed record ListPluginInstallationsQuery(Guid CompanyId, int Page = 1, int PageSize = 25)
    : IQuery<PagedResult<PluginInstallationDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "marketplace.plugin-installation.read" };
}
