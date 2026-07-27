using FusionOS.Modules.Inventory.Application.Attributes.Contracts;
using FusionOS.Modules.Inventory.Domain.Attributes;
using FusionOS.Modules.Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Inventory.Infrastructure.Repositories;

// NOTE: References InventoryDbContext.AttributeDefinitions, a DbSet that must
// be added to InventoryDbContext.cs by whoever owns that shared file (this
// task was told not to edit it directly) — see the task's final report for
// the exact line needed.
public sealed class AttributeDefinitionRepository : IAttributeDefinitionRepository
{
    private readonly InventoryDbContext _context;

    public AttributeDefinitionRepository(InventoryDbContext context) => _context = context;

    public Task<bool> NameExistsAsync(Guid companyId, string name, CancellationToken cancellationToken = default) =>
        _context.AttributeDefinitions.AnyAsync(a => a.CompanyId == companyId && EF.Functions.ILike(a.Name, name.Trim()), cancellationToken);

    public Task<AttributeDefinition?> GetByIdAsync(Guid companyId, Guid attributeDefinitionId, CancellationToken cancellationToken = default) =>
        _context.AttributeDefinitions.FirstOrDefaultAsync(a => a.CompanyId == companyId && a.Id == attributeDefinitionId, cancellationToken);

    public async Task AddAsync(AttributeDefinition attributeDefinition, CancellationToken cancellationToken = default) =>
        await _context.AttributeDefinitions.AddAsync(attributeDefinition, cancellationToken);

    public async Task<IReadOnlyList<AttributeDefinition>> ListAsync(Guid companyId, string? search, int page, int pageSize, CancellationToken cancellationToken = default) =>
        await Filtered(companyId, search)
            .OrderBy(a => a.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(Guid companyId, string? search, CancellationToken cancellationToken = default) =>
        Filtered(companyId, search).CountAsync(cancellationToken);

    private IQueryable<AttributeDefinition> Filtered(Guid companyId, string? search)
    {
        var query = _context.AttributeDefinitions.Where(a => a.CompanyId == companyId);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(a => EF.Functions.ILike(a.Name, pattern));
        }
        return query;
    }
}
