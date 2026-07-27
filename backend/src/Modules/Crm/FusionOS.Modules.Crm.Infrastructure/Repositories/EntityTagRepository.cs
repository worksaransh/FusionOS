using FusionOS.Modules.Crm.Application.Tags.Contracts;
using FusionOS.Modules.Crm.Domain.Tags;
using FusionOS.Modules.Crm.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Crm.Infrastructure.Repositories;

public sealed class EntityTagRepository : IEntityTagRepository
{
    private readonly CrmDbContext _context;

    public EntityTagRepository(CrmDbContext context) => _context = context;

    public Task<EntityTag?> FindAsync(Guid companyId, Guid tagId, string entityType, Guid entityId, CancellationToken cancellationToken = default) =>
        _context.Set<EntityTag>().FirstOrDefaultAsync(
            t => t.CompanyId == companyId && t.TagId == tagId && t.EntityType == entityType && t.EntityId == entityId,
            cancellationToken);

    public async Task AddAsync(EntityTag entityTag, CancellationToken cancellationToken = default) =>
        await _context.Set<EntityTag>().AddAsync(entityTag, cancellationToken);

    public void Remove(EntityTag entityTag) => _context.Set<EntityTag>().Remove(entityTag);

    public async Task<IReadOnlyList<EntityTag>> ListForEntityAsync(Guid companyId, string entityType, Guid entityId, CancellationToken cancellationToken = default) =>
        await _context.Set<EntityTag>()
            .Where(t => t.CompanyId == companyId && t.EntityType == entityType && t.EntityId == entityId)
            .ToListAsync(cancellationToken);
}
