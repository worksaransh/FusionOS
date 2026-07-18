using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Core.Application.Settings.Contracts;

namespace FusionOS.Modules.Core.Application.Settings.Queries.GetCompanySettings;

/// <summary>Read-gated (Phase M5, 2026-07-15) — requires "core.settings.read".</summary>
public sealed record GetCompanySettingsQuery(Guid CompanyId) : IQuery<CompanySettingsDto>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "core.settings.read" };
}
