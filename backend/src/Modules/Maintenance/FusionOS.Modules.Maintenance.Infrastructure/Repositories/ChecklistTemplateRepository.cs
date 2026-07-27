using FusionOS.Modules.Maintenance.Application.ChecklistTemplates.Contracts;
using FusionOS.Modules.Maintenance.Domain.ChecklistTemplates;
using FusionOS.Modules.Maintenance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Maintenance.Infrastructure.Repositories;

public sealed class ChecklistTemplateRepository : IChecklistTemplateRepository
{
    private readonly MaintenanceDbContext _context;

    public ChecklistTemplateRepository(MaintenanceDbContext context) => _context = context;

    // .Include(x => x.Items) is required because the collection is backed by a private field.
    public Task<ChecklistTemplate?> GetByIdAsync(Guid companyId, Guid checklistTemplateId, CancellationToken cancellationToken = default) =>
        _context.ChecklistTemplates.Include(t => t.Items).FirstOrDefaultAsync(t => t.CompanyId == companyId && t.Id == checklistTemplateId, cancellationToken);

    public async Task AddAsync(ChecklistTemplate template, CancellationToken cancellationToken = default) =>
        await _context.ChecklistTemplates.AddAsync(template, cancellationToken);

    public async Task<IReadOnlyList<ChecklistTemplate>> ListAsync(Guid companyId, string? search, int page, int pageSize, CancellationToken cancellationToken = default) =>
        await Filtered(companyId, search)
            .Include(t => t.Items)
            .OrderBy(t => t.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(Guid companyId, string? search, CancellationToken cancellationToken = default) =>
        Filtered(companyId, search).CountAsync(cancellationToken);

    private IQueryable<ChecklistTemplate> Filtered(Guid companyId, string? search)
    {
        var query = _context.ChecklistTemplates.Where(t => t.CompanyId == companyId);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(t => EF.Functions.ILike(t.Name, pattern));
        }
        return query;
    }
}
