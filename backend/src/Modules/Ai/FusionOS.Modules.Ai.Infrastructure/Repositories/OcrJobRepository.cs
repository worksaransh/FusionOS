using FusionOS.Modules.Ai.Application.OcrJobs.Contracts;
using FusionOS.Modules.Ai.Domain.OcrJobs;
using FusionOS.Modules.Ai.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Ai.Infrastructure.Repositories;

public sealed class OcrJobRepository : IOcrJobRepository
{
    private readonly AiDbContext _context;

    public OcrJobRepository(AiDbContext context) => _context = context;

    public Task<OcrJob?> GetByIdAsync(Guid companyId, Guid ocrJobId, CancellationToken cancellationToken = default) =>
        _context.OcrJobs.FirstOrDefaultAsync(j => j.CompanyId == companyId && j.Id == ocrJobId, cancellationToken);

    public async Task AddAsync(OcrJob job, CancellationToken cancellationToken = default) =>
        await _context.OcrJobs.AddAsync(job, cancellationToken);

    public async Task<IReadOnlyList<OcrJob>> ListAsync(Guid companyId, OcrJobStatus? status, int page, int pageSize, CancellationToken cancellationToken = default) =>
        await Filtered(companyId, status)
            .OrderByDescending(j => j.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(Guid companyId, OcrJobStatus? status, CancellationToken cancellationToken = default) =>
        Filtered(companyId, status).CountAsync(cancellationToken);

    private IQueryable<OcrJob> Filtered(Guid companyId, OcrJobStatus? status)
    {
        var query = _context.OcrJobs.Where(j => j.CompanyId == companyId);
        if (status.HasValue)
            query = query.Where(j => j.Status == status.Value);
        return query;
    }
}
