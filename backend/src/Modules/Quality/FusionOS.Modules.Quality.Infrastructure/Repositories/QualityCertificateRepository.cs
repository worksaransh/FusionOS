using FusionOS.Modules.Quality.Application.Certificates.Contracts;
using FusionOS.Modules.Quality.Domain.Certificates;
using FusionOS.Modules.Quality.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Quality.Infrastructure.Repositories;

/// <summary>
/// Uses context.Set&lt;QualityCertificate&gt;() rather than a QualityDbContext.QualityCertificates
/// DbSet property — QualityDbContext.cs is owned by a concurrent session for this slice, but EF
/// Core discovers QualityCertificateConfiguration via ApplyConfigurationsFromAssembly (already
/// wired in QualityDbContext.OnModelCreating) with no DbSet property required.
/// </summary>
public sealed class QualityCertificateRepository : IQualityCertificateRepository
{
    private readonly QualityDbContext _context;

    public QualityCertificateRepository(QualityDbContext context) => _context = context;

    private DbSet<QualityCertificate> Certificates => _context.Set<QualityCertificate>();

    public Task<QualityCertificate?> GetByIdAsync(Guid companyId, Guid certificateId, CancellationToken cancellationToken = default) =>
        Certificates.FirstOrDefaultAsync(c => c.CompanyId == companyId && c.Id == certificateId, cancellationToken);

    public Task<QualityCertificate?> GetByInspectionIdAsync(Guid companyId, Guid inspectionId, CancellationToken cancellationToken = default) =>
        Certificates.FirstOrDefaultAsync(c => c.CompanyId == companyId && c.InspectionId == inspectionId, cancellationToken);

    public async Task AddAsync(QualityCertificate certificate, CancellationToken cancellationToken = default) =>
        await Certificates.AddAsync(certificate, cancellationToken);

    public async Task<IReadOnlyList<QualityCertificate>> ListAsync(Guid companyId, int page, int pageSize, CancellationToken cancellationToken = default) =>
        await Certificates
            .Where(c => c.CompanyId == companyId)
            .OrderByDescending(c => c.IssuedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(Guid companyId, CancellationToken cancellationToken = default) =>
        Certificates.CountAsync(c => c.CompanyId == companyId, cancellationToken);
}
