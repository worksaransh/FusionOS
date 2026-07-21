using FusionOS.Modules.Quality.Application.CorrectiveActions.Contracts;
using FusionOS.Modules.Quality.Domain.CorrectiveActions;
using FusionOS.Modules.Quality.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Quality.Infrastructure.Repositories;

public sealed class CorrectiveActionRepository : ICorrectiveActionRepository
{
    private readonly QualityDbContext _context;

    public CorrectiveActionRepository(QualityDbContext context) => _context = context;

    public Task<CorrectiveAction?> GetByIdAsync(Guid companyId, Guid correctiveActionId, CancellationToken cancellationToken = default) =>
        _context.CorrectiveActions.FirstOrDefaultAsync(c => c.CompanyId == companyId && c.Id == correctiveActionId, cancellationToken);

    public async Task AddAsync(CorrectiveAction correctiveAction, CancellationToken cancellationToken = default) =>
        await _context.CorrectiveActions.AddAsync(correctiveAction, cancellationToken);

    public async Task<IReadOnlyList<CorrectiveAction>> ListAsync(Guid companyId, Guid? nonConformanceReportId, int page, int pageSize, CancellationToken cancellationToken = default) =>
        await Filtered(companyId, nonConformanceReportId)
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(Guid companyId, Guid? nonConformanceReportId, CancellationToken cancellationToken = default) =>
        Filtered(companyId, nonConformanceReportId).CountAsync(cancellationToken);

    private IQueryable<CorrectiveAction> Filtered(Guid companyId, Guid? nonConformanceReportId)
    {
        var query = _context.CorrectiveActions.Where(c => c.CompanyId == companyId);
        if (nonConformanceReportId.HasValue)
            query = query.Where(c => c.NonConformanceReportId == nonConformanceReportId.Value);
        return query;
    }
}
