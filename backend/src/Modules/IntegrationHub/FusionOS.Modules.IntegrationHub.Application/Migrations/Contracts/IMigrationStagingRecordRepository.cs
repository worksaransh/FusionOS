using FusionOS.Modules.IntegrationHub.Domain.Migrations;

namespace FusionOS.Modules.IntegrationHub.Application.Migrations.Contracts;

public interface IMigrationStagingRecordRepository
{
    Task<Domain.Migrations.MigrationStagingRecord?> GetByIdAsync(Guid companyId, Guid stagingRecordId, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.Migrations.MigrationStagingRecord record, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<Domain.Migrations.MigrationStagingRecord> records, CancellationToken cancellationToken = default);

    /// <summary>All staging records for a job, optionally paged — used by the staging-records list query.</summary>
    Task<IReadOnlyList<Domain.Migrations.MigrationStagingRecord>> ListByJobIdAsync(Guid companyId, Guid migrationJobId, MappingStatus? mappingStatus, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountByJobIdAsync(Guid companyId, Guid migrationJobId, MappingStatus? mappingStatus, CancellationToken cancellationToken = default);

    /// <summary>Unpaged — used by the pipeline stages (Validate/Load) to process every record in a given status for a job in one pass.</summary>
    Task<IReadOnlyList<Domain.Migrations.MigrationStagingRecord>> ListByJobIdAndStatusAsync(Guid companyId, Guid migrationJobId, MappingStatus mappingStatus, CancellationToken cancellationToken = default);
}
