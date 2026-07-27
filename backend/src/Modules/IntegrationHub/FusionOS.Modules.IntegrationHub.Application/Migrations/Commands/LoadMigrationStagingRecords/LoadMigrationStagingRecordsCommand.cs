using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.IntegrationHub.Application.Migrations.Contracts;

namespace FusionOS.Modules.IntegrationHub.Application.Migrations.Commands.LoadMigrationStagingRecords;

/// <summary>
/// The FusionOS Load step (11_SAP_MIGRATION.md §2) — "performed through the same
/// Application-layer commands/APIs real users would use (not direct SQL inserts
/// bypassing domain invariants)". This pass wires exactly one real path for real:
/// Tally LEDGER → Finance's CreateAccountCommand. STOCKITEM → Inventory Product and
/// VOUCHER → Finance Journal Entry are the same pattern, straightforwardly repeatable,
/// but not implemented here — see the handler's class doc comment.
/// </summary>
public sealed record LoadMigrationStagingRecordsCommand(Guid CompanyId, Guid MigrationJobId)
    : ICommand<MigrationJobDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "integration_hub.migration.run" };
    public string EntityType => nameof(Domain.Migrations.MigrationJob);
    public Guid EntityId => MigrationJobId;
    public string Action => "StagingRecordsLoaded";
}
