using FusionOS.Modules.IntegrationHub.Application.Migrations.Contracts;
using FusionOS.Modules.IntegrationHub.Domain.Migrations;
using FusionOS.Modules.IntegrationHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.IntegrationHub.Infrastructure.Repositories;

public sealed class MigrationJobRepository : IMigrationJobRepository
{
    private readonly IntegrationHubDbContext _context;

    public MigrationJobRepository(IntegrationHubDbContext context) => _context = context;

    public Task<MigrationJob?> GetByIdAsync(Guid companyId, Guid migrationJobId, CancellationToken cancellationToken = default) =>
        _context.MigrationJobs.FirstOrDefaultAsync(j => j.CompanyId == companyId && j.Id == migrationJobId, cancellationToken);

    public Task<bool> ExistsAsync(Guid companyId, Guid migrationJobId, CancellationToken cancellationToken = default) =>
        _context.MigrationJobs.AnyAsync(j => j.CompanyId == companyId && j.Id == migrationJobId, cancellationToken);

    public async Task AddAsync(MigrationJob job, CancellationToken cancellationToken = default) =>
        await _context.MigrationJobs.AddAsync(job, cancellationToken);

    public async Task<IReadOnlyList<MigrationJob>> ListAsync(Guid companyId, int page, int pageSize, CancellationToken cancellationToken = default) =>
        await _context.MigrationJobs
            .Where(j => j.CompanyId == companyId)
            .OrderByDescending(j => j.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(Guid companyId, CancellationToken cancellationToken = default) =>
        _context.MigrationJobs.CountAsync(j => j.CompanyId == companyId, cancellationToken);
}
