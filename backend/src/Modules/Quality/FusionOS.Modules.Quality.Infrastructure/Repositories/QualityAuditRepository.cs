using FusionOS.Modules.Quality.Application.Audits.Contracts;
using FusionOS.Modules.Quality.Domain.Audits;
using FusionOS.Modules.Quality.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Quality.Infrastructure.Repositories;

public sealed class QualityAuditRepository : IQualityAuditRepository
{
    private readonly QualityDbContext _context;

    public QualityAuditRepository(QualityDbContext context) => _context = context;

    // .Include(x => x.Findings) is required because the Findings collection is backed by a private field.
    public Task<QualityAudit?> GetByIdAsync(Guid companyId, Guid qualityAuditId, CancellationToken cancellationToken = default) =>
        _context.QualityAudits.Include(a => a.Findings).FirstOrDefaultAsync(a => a.CompanyId == companyId && a.Id == qualityAuditId, cancellationToken);

    public async Task AddAsync(QualityAudit qualityAudit, CancellationToken cancellationToken = default) =>
        await _context.QualityAudits.AddAsync(qualityAudit, cancellationToken);

    public async Task<IReadOnlyList<QualityAudit>> ListAsync(Guid companyId, int page, int pageSize, CancellationToken cancellationToken = default) =>
        await _context.QualityAudits
            .Include(a => a.Findings)
            .Where(a => a.CompanyId == companyId)
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(Guid companyId, CancellationToken cancellationToken = default) =>
        _context.QualityAudits.CountAsync(a => a.CompanyId == companyId, cancellationToken);
}
