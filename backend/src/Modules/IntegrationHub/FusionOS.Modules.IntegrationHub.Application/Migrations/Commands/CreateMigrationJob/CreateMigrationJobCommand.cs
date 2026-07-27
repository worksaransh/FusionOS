using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.IntegrationHub.Application.Migrations.Contracts;

namespace FusionOS.Modules.IntegrationHub.Application.Migrations.Commands.CreateMigrationJob;

public sealed record CreateMigrationJobCommand(Guid CompanyId, string SourceSystem)
    : ICommand<MigrationJobDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "integration_hub.migration.create" };
    public string EntityType => nameof(Domain.Migrations.MigrationJob);
    public Guid EntityId { get; init; }
    public string Action => "Created";
}
