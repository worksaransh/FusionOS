using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.IntegrationHub.Application.Migrations.Contracts;

namespace FusionOS.Modules.IntegrationHub.Application.Migrations.Commands.ExtractTallyData;

/// <summary>
/// Runs the Tally extractor against a pasted/uploaded Tally XML export and lands every
/// recognized record as a MigrationStagingRecord (11_SAP_MIGRATION.md §2's
/// "Source System → Extractor → Staging Schema" step). A future non-Tally source (CSV,
/// generic SQL, Odoo REST) would plug in as a sibling command with the same shape —
/// "parse the source into (RecordType, RawDataJson) pairs, stage them, CompleteExtraction" —
/// but only Tally is implemented for real this pass, per §4's stated priority.
/// </summary>
public sealed record ExtractTallyDataCommand(Guid CompanyId, Guid MigrationJobId, string TallyXmlContent)
    : ICommand<MigrationJobDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "integration_hub.migration.run" };
    public string EntityType => nameof(Domain.Migrations.MigrationJob);
    public Guid EntityId => MigrationJobId;
    public string Action => "TallyExtracted";
}
