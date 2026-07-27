using FusionOS.Modules.Maintenance.Application.MeterReadings.Contracts;
using FusionOS.Modules.Maintenance.Domain.MeterReadings;
using FusionOS.Modules.Maintenance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Maintenance.Infrastructure.Repositories;

public sealed class MeterReadingRepository : IMeterReadingRepository
{
    private readonly MaintenanceDbContext _context;

    public MeterReadingRepository(MaintenanceDbContext context) => _context = context;

    public async Task AddAsync(MeterReading reading, CancellationToken cancellationToken = default) =>
        await _context.MeterReadings.AddAsync(reading, cancellationToken);

    public Task<MeterReading?> GetLatestForAssetAsync(Guid companyId, Guid assetId, CancellationToken cancellationToken = default) =>
        _context.MeterReadings
            .Where(r => r.CompanyId == companyId && r.AssetId == assetId)
            .OrderByDescending(r => r.ReadingDate)
            .ThenByDescending(r => r.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<MeterReading>> ListForAssetAsync(Guid companyId, Guid assetId, int page, int pageSize, CancellationToken cancellationToken = default) =>
        await Filtered(companyId, assetId)
            .OrderByDescending(r => r.ReadingDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountForAssetAsync(Guid companyId, Guid assetId, CancellationToken cancellationToken = default) =>
        Filtered(companyId, assetId).CountAsync(cancellationToken);

    private IQueryable<MeterReading> Filtered(Guid companyId, Guid assetId) =>
        _context.MeterReadings.Where(r => r.CompanyId == companyId && r.AssetId == assetId);
}
