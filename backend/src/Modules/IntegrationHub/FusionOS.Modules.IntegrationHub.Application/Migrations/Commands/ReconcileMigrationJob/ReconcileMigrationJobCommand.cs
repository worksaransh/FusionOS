using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.IntegrationHub.Application.Migrations.Contracts;

namespace FusionOS.Modules.IntegrationHub.Application.Migrations.Commands.ReconcileMigrationJob;

/// <summary>The Reconciliation step (11_SAP_MIGRATION.md §2/§6) — compares extracted vs. loaded+failed totals and records an honest, human-readable summary.</summary>
public sealed record ReconcileMigrationJobCommand(Guid CompanyId, Guid MigrationJobId)
    : ICommand<MigrationJobDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "integration_hub.migration.run" };
    public string EntityType => nameof(Domain.Migrations.MigrationJob);
    public Guid EntityId => MigrationJobId;
    public string Action => "Reconciled";
}
