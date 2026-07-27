using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.IntegrationHub.Application.Migrations.Contracts;
using FusionOS.Modules.IntegrationHub.Domain.Migrations;

namespace FusionOS.Modules.IntegrationHub.Application.Migrations.Queries.ListMigrationStagingRecordsForJob;

public sealed record ListMigrationStagingRecordsForJobQuery(Guid CompanyId, Guid MigrationJobId, MappingStatus? MappingStatus = null, int Page = 1, int PageSize = 50)
    : IQuery<PagedResult<MigrationStagingRecordDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "integration_hub.migration.read" };
}
