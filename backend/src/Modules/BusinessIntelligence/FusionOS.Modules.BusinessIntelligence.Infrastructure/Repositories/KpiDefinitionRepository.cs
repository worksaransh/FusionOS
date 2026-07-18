using FusionOS.Modules.BusinessIntelligence.Application.KpiDefinitions.Contracts;
using FusionOS.Modules.BusinessIntelligence.Domain.KpiDefinitions;
using FusionOS.Modules.BusinessIntelligence.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.BusinessIntelligence.Infrastructure.Repositories;

public sealed class KpiDefinitionRepository : IKpiDefinitionRepository
{
    private readonly BusinessIntelligenceDbContext _context;

    public KpiDefinitionRepository(BusinessIntelligenceDbContext context) => _context = context;

    public Task<KpiDefinition?> GetByIdAsync(Guid companyId, Guid kpiDefinitionId, CancellationToken cancellationToken = default) =>
        _context.KpiDefinitions.FirstOrDefaultAsync(k => k.CompanyId == companyId && k.Id == kpiDefinitionId, cancellationToken);

    public Task<bool> ExistsAsync(Guid companyId, Guid kpiDefinitionId, CancellationToken cancellationToken = default) =>
        _context.KpiDefinitions.AnyAsync(k => k.CompanyId == companyId && k.Id == kpiDefinitionId, cancellationToken);

    public async Task AddAsync(KpiDefinition kpiDefinition, CancellationToken cancellationToken = default) =>
        await _context.KpiDefinitions.AddAsync(kpiDefinition, cancellationToken);

    public async Task<IReadOnlyList<KpiDefinition>> ListAsync(Guid companyId, string? search, int page, int pageSize, CancellationToken cancellationToken = default) =>
        await Filtered(companyId, search)
            .OrderBy(k => k.Code)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(Guid companyId, string? search, CancellationToken cancellationToken = default) =>
        Filtered(companyId, search).CountAsync(cancellationToken);

    private IQueryable<KpiDefinition> Filtered(Guid companyId, string? search)
    {
        var query = _context.KpiDefinitions.Where(k => k.CompanyId == companyId);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(k => EF.Functions.ILike(k.Code, pattern) || EF.Functions.ILike(k.Name, pattern));
        }
        return query;
    }
}
