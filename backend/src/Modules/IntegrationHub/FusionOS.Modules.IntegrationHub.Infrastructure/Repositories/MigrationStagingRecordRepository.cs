using FusionOS.Modules.IntegrationHub.Application.Migrations.Contracts;
using FusionOS.Modules.IntegrationHub.Domain.Migrations;
using FusionOS.Modules.IntegrationHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.IntegrationHub.Infrastructure.Repositories;

public sealed class MigrationStagingRecordRepository : IMigrationStagingRecordRepository
{
    private readonly IntegrationHubDbContext _context;

    public MigrationStagingRecordRepository(IntegrationHubDbContext context) => _context = context;

    public Task<MigrationStagingRecord?> GetByIdAsync(Guid companyId, Guid stagingRecordId, CancellationToken cancellationToken = default) =>
        _context.MigrationStagingRecords.FirstOrDefaultAsync(r => r.CompanyId == companyId && r.Id == stagingRecordId, cancellationToken);

    public async Task AddAsync(MigrationStagingRecord record, CancellationToken cancellationToken = default) =>
        await _context.MigrationStagingRecords.AddAsync(record, cancellationToken);

    public async Task AddRangeAsync(IEnumerable<MigrationStagingRecord> records, CancellationToken cancellationToken = default) =>
        await _context.MigrationStagingRecords.AddRangeAsync(records, cancellationToken);

    public async Task<IReadOnlyList<MigrationStagingRecord>> ListByJobIdAsync(Guid companyId, Guid migrationJobId, MappingStatus? mappingStatus, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.MigrationStagingRecords
            .Where(r => r.CompanyId == companyId && r.MigrationJobId == migrationJobId);

        if (mappingStatus is { } status)
            query = query.Where(r => r.MappingStatus == status);

        return await query
            .OrderBy(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountByJobIdAsync(Guid companyId, Guid migrationJobId, MappingStatus? mappingStatus, CancellationToken cancellationToken = default)
    {
        var query = _context.MigrationStagingRecords
            .Where(r => r.CompanyId == companyId && r.MigrationJobId == migrationJobId);

        if (mappingStatus is { } status)
            query = query.Where(r => r.MappingStatus == status);

        return query.CountAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<MigrationStagingRecord>> ListByJobIdAndStatusAsync(Guid companyId, Guid migrationJobId, MappingStatus mappingStatus, CancellationToken cancellationToken = default) =>
        await _context.MigrationStagingRecords
            .Where(r => r.CompanyId == companyId && r.MigrationJobId == migrationJobId && r.MappingStatus == mappingStatus)
            .ToListAsync(cancellationToken);
}
