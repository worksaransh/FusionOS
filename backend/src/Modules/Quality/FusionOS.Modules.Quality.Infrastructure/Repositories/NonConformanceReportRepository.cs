using FusionOS.Modules.Quality.Application.NonConformanceReports.Contracts;
using FusionOS.Modules.Quality.Domain.NonConformanceReports;
using FusionOS.Modules.Quality.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Quality.Infrastructure.Repositories;

public sealed class NonConformanceReportRepository : INonConformanceReportRepository
{
    private readonly QualityDbContext _context;

    public NonConformanceReportRepository(QualityDbContext context) => _context = context;

    public Task<NonConformanceReport?> GetByIdAsync(Guid companyId, Guid nonConformanceReportId, CancellationToken cancellationToken = default) =>
        _context.NonConformanceReports.FirstOrDefaultAsync(r => r.CompanyId == companyId && r.Id == nonConformanceReportId, cancellationToken);

    public async Task AddAsync(NonConformanceReport report, CancellationToken cancellationToken = default) =>
        await _context.NonConformanceReports.AddAsync(report, cancellationToken);

    public async Task<IReadOnlyList<NonConformanceReport>> ListAsync(Guid companyId, Guid? inspectionId, int page, int pageSize, CancellationToken cancellationToken = default) =>
        await Filtered(companyId, inspectionId)
            .OrderByDescending(r => r.RaisedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(Guid companyId, Guid? inspectionId, CancellationToken cancellationToken = default) =>
        Filtered(companyId, inspectionId).CountAsync(cancellationToken);

    private IQueryable<NonConformanceReport> Filtered(Guid companyId, Guid? inspectionId)
    {
        var query = _context.NonConformanceReports.Where(r => r.CompanyId == companyId);
        if (inspectionId.HasValue)
            query = query.Where(r => r.InspectionId == inspectionId.Value);
        return query;
    }
}
