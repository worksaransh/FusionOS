using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.IntegrationHub.Application.Migrations.Contracts;

namespace FusionOS.Modules.IntegrationHub.Application.Migrations.Commands.ValidateMigrationStagingRecords;

/// <summary>
/// Runs simple, honest per-record-type validation over every Pending staging record for a
/// job (11_SAP_MIGRATION.md §2's Validation step — "failures are reported per-record with
/// the reason, never silently dropped or silently coerced"). This is deliberately NOT
/// exhaustive business-rule validation (the doc's long-term vision is running the same
/// FluentValidation rules real transactions go through, once field-mapping/transform
/// exists) — it checks the minimum a record needs to even attempt a Load.
/// </summary>
public sealed record ValidateMigrationStagingRecordsCommand(Guid CompanyId, Guid MigrationJobId)
    : ICommand<MigrationJobDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "integration_hub.migration.run" };
    public string EntityType => nameof(Domain.Migrations.MigrationJob);
    public Guid EntityId => MigrationJobId;
    public string Action => "StagingRecordsValidated";
}
