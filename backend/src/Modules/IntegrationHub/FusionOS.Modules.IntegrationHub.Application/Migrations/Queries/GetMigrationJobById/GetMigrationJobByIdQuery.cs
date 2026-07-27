using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.IntegrationHub.Application.Migrations.Contracts;

namespace FusionOS.Modules.IntegrationHub.Application.Migrations.Queries.GetMigrationJobById;

public sealed record GetMigrationJobByIdQuery(Guid CompanyId, Guid MigrationJobId) : IQuery<MigrationJobDto>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "integration_hub.migration.read" };
}
