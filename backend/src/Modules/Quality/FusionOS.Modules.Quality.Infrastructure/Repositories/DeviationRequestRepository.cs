using FusionOS.Modules.Quality.Application.Deviations.Contracts;
using FusionOS.Modules.Quality.Domain.Deviations;
using FusionOS.Modules.Quality.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Quality.Infrastructure.Repositories;

public sealed class DeviationRequestRepository : IDeviationRequestRepository
{
    private readonly QualityDbContext _context;

    public DeviationRequestRepository(QualityDbContext context) => _context = context;

    public Task<DeviationRequest?> GetByIdAsync(Guid companyId, Guid deviationRequestId, CancellationToken cancellationToken = default) =>
        _context.DeviationRequests.FirstOrDefaultAsync(d => d.CompanyId == companyId && d.Id == deviationRequestId, cancellationToken);

    public async Task AddAsync(DeviationRequest deviationRequest, CancellationToken cancellationToken = default) =>
        await _context.DeviationRequests.AddAsync(deviationRequest, cancellationToken);

    public async Task<IReadOnlyList<DeviationRequest>> ListAsync(Guid companyId, Guid? inspectionId, int page, int pageSize, CancellationToken cancellationToken = default) =>
        await Filtered(companyId, inspectionId)
            .OrderByDescending(d => d.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(Guid companyId, Guid? inspectionId, CancellationToken cancellationToken = default) =>
        Filtered(companyId, inspectionId).CountAsync(cancellationToken);

    private IQueryable<DeviationRequest> Filtered(Guid companyId, Guid? inspectionId)
    {
        var query = _context.DeviationRequests.Where(d => d.CompanyId == companyId);
        if (inspectionId.HasValue)
            query = query.Where(d => d.InspectionId == inspectionId.Value);
        return query;
    }
}
