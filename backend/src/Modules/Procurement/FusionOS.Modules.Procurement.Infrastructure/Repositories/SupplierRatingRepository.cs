using FusionOS.Modules.Procurement.Application.SupplierRatings.Contracts;
using FusionOS.Modules.Procurement.Domain.SupplierRatings;
using FusionOS.Modules.Procurement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Procurement.Infrastructure.Repositories;

public sealed class SupplierRatingRepository : ISupplierRatingRepository
{
    private readonly ProcurementDbContext _context;

    public SupplierRatingRepository(ProcurementDbContext context) => _context = context;

    public Task<SupplierRating?> GetByIdAsync(Guid companyId, Guid id, CancellationToken cancellationToken = default) =>
        _context.SupplierRatings.FirstOrDefaultAsync(x => x.CompanyId == companyId && x.Id == id, cancellationToken);

    public async Task AddAsync(SupplierRating rating, CancellationToken cancellationToken = default) =>
        await _context.SupplierRatings.AddAsync(rating, cancellationToken);

    public async Task<IReadOnlyList<SupplierRating>> ListBySupplierAsync(Guid companyId, Guid supplierId, int page, int pageSize, CancellationToken cancellationToken = default) =>
        await _context.SupplierRatings
            .Where(x => x.CompanyId == companyId && x.SupplierId == supplierId)
            .OrderByDescending(x => x.RatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountBySupplierAsync(Guid companyId, Guid supplierId, CancellationToken cancellationToken = default) =>
        _context.SupplierRatings.CountAsync(x => x.CompanyId == companyId && x.SupplierId == supplierId, cancellationToken);

    public async Task<IReadOnlyList<(Guid SupplierId, double AverageQualityScore, double AverageDeliveryScore, int RatingCount)>> GetSupplierRatingStatsAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        var grouped = await _context.SupplierRatings
            .Where(x => x.CompanyId == companyId)
            .GroupBy(x => x.SupplierId)
            .Select(g => new
            {
                SupplierId = g.Key,
                AverageQualityScore = g.Average(r => (double)r.QualityScore),
                AverageDeliveryScore = g.Average(r => (double)r.DeliveryScore),
                RatingCount = g.Count(),
            })
            .ToListAsync(cancellationToken);

        return grouped.Select(g => (g.SupplierId, g.AverageQualityScore, g.AverageDeliveryScore, g.RatingCount)).ToList();
    }
}
