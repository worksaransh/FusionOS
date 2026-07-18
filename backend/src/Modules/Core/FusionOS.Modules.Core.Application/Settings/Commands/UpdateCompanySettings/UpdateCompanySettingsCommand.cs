using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Core.Application.Settings.Contracts;

namespace FusionOS.Modules.Core.Application.Settings.Commands.UpdateCompanySettings;

/// <summary>Write-gated (Phase M5, 2026-07-15) — requires "core.settings.update".</summary>
public sealed record UpdateCompanySettingsCommand(Guid CompanyId, string DefaultCurrency, int DefaultPageSize, string? DisplayName, string? LogoUrl)
    : ICommand<CompanySettingsDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "core.settings.update" };
    public string EntityType => nameof(Domain.Settings.CompanySettings);
    public Guid EntityId { get; init; }
    public string Action => "Updated";
}
