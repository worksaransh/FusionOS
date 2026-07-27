using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.IntegrationHub.Application.Migrations.Contracts;

namespace FusionOS.Modules.IntegrationHub.Application.Migrations.Queries.ListMigrationJobs;

public sealed record ListMigrationJobsQuery(Guid CompanyId, int Page = 1, int PageSize = 25)
    : IQuery<PagedResult<MigrationJobDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "integration_hub.migration.read" };
}
