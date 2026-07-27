using FusionOS.Modules.Crm.Application.Tags.Contracts;
using FusionOS.Modules.Crm.Domain.Tags;
using FusionOS.Modules.Crm.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Crm.Infrastructure.Repositories;

public sealed class TagRepository : ITagRepository
{
    private readonly CrmDbContext _context;

    public TagRepository(CrmDbContext context) => _context = context;

    public Task<Tag?> GetByIdAsync(Guid companyId, Guid tagId, CancellationToken cancellationToken = default) =>
        _context.Set<Tag>().FirstOrDefaultAsync(t => t.CompanyId == companyId && t.Id == tagId, cancellationToken);

    public Task<bool> ExistsAsync(Guid companyId, Guid tagId, CancellationToken cancellationToken = default) =>
        _context.Set<Tag>().AnyAsync(t => t.CompanyId == companyId && t.Id == tagId, cancellationToken);

    public Task<bool> ExistsByNameAsync(Guid companyId, string name, CancellationToken cancellationToken = default) =>
        _context.Set<Tag>().AnyAsync(t => t.CompanyId == companyId && t.Name.ToLower() == name.Trim().ToLower(), cancellationToken);

    public async Task AddAsync(Tag tag, CancellationToken cancellationToken = default) =>
        await _context.Set<Tag>().AddAsync(tag, cancellationToken);

    public async Task<IReadOnlyList<Tag>> ListAsync(Guid companyId, string? search, int page, int pageSize, CancellationToken cancellationToken = default) =>
        await Filtered(companyId, search)
            .OrderBy(t => t.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(Guid companyId, string? search, CancellationToken cancellationToken = default) =>
        Filtered(companyId, search).CountAsync(cancellationToken);

    public async Task<IReadOnlyList<Tag>> GetByIdsAsync(Guid companyId, IReadOnlyCollection<Guid> tagIds, CancellationToken cancellationToken = default) =>
        await _context.Set<Tag>().Where(t => t.CompanyId == companyId && tagIds.Contains(t.Id)).ToListAsync(cancellationToken);

    private IQueryable<Tag> Filtered(Guid companyId, string? search)
    {
        var query = _context.Set<Tag>().Where(t => t.CompanyId == companyId);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(t => EF.Functions.ILike(t.Name, pattern));
        }

        return query;
    }
}
