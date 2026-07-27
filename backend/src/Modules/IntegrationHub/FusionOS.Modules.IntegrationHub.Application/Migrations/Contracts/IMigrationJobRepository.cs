namespace FusionOS.Modules.IntegrationHub.Application.Migrations.Contracts;

public interface IMigrationJobRepository
{
    Task<Domain.Migrations.MigrationJob?> GetByIdAsync(Guid companyId, Guid migrationJobId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid companyId, Guid migrationJobId, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.Migrations.MigrationJob job, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.Migrations.MigrationJob>> ListAsync(Guid companyId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid companyId, CancellationToken cancellationToken = default);
}
