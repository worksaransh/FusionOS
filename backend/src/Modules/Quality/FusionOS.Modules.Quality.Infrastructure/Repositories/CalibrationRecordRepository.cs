using FusionOS.Modules.Quality.Application.Calibrations.Contracts;
using FusionOS.Modules.Quality.Domain.Calibrations;
using FusionOS.Modules.Quality.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Quality.Infrastructure.Repositories;

/// <summary>
/// Uses context.Set&lt;CalibrationRecord&gt;() rather than a QualityDbContext.CalibrationRecords
/// DbSet property — see QualityCertificateRepository's doc comment for why.
/// </summary>
public sealed class CalibrationRecordRepository : ICalibrationRecordRepository
{
    private readonly QualityDbContext _context;

    public CalibrationRecordRepository(QualityDbContext context) => _context = context;

    private DbSet<CalibrationRecord> CalibrationRecords => _context.Set<CalibrationRecord>();

    public Task<CalibrationRecord?> GetByIdAsync(Guid companyId, Guid calibrationRecordId, CancellationToken cancellationToken = default) =>
        CalibrationRecords.FirstOrDefaultAsync(c => c.CompanyId == companyId && c.Id == calibrationRecordId, cancellationToken);

    public async Task AddAsync(CalibrationRecord record, CancellationToken cancellationToken = default) =>
        await CalibrationRecords.AddAsync(record, cancellationToken);

    public async Task<IReadOnlyList<CalibrationRecord>> ListAsync(Guid companyId, bool overdueOnly, int page, int pageSize, CancellationToken cancellationToken = default) =>
        await Filtered(companyId, overdueOnly)
            .OrderBy(c => c.NextCalibrationDueDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(Guid companyId, bool overdueOnly, CancellationToken cancellationToken = default) =>
        Filtered(companyId, overdueOnly).CountAsync(cancellationToken);

    private IQueryable<CalibrationRecord> Filtered(Guid companyId, bool overdueOnly)
    {
        var query = CalibrationRecords.Where(c => c.CompanyId == companyId);
        if (overdueOnly)
        {
            var now = DateTimeOffset.UtcNow;
            query = query.Where(c => c.NextCalibrationDueDate < now);
        }
        return query;
    }
}
