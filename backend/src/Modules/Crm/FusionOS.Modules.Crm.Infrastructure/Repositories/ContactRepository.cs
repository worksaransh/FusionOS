using FusionOS.Modules.Crm.Application.Contacts.Contracts;
using FusionOS.Modules.Crm.Domain.Contacts;
using FusionOS.Modules.Crm.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Crm.Infrastructure.Repositories;

public sealed class ContactRepository : IContactRepository
{
    private readonly CrmDbContext _context;

    public ContactRepository(CrmDbContext context) => _context = context;

    public Task<Contact?> GetByIdAsync(Guid companyId, Guid contactId, CancellationToken cancellationToken = default) =>
        _context.Contacts.FirstOrDefaultAsync(c => c.CompanyId == companyId && c.Id == contactId, cancellationToken);

    public async Task AddAsync(Contact contact, CancellationToken cancellationToken = default) =>
        await _context.Contacts.AddAsync(contact, cancellationToken);

    public async Task<IReadOnlyList<Contact>> ListAsync(Guid companyId, string? search, int page, int pageSize, CancellationToken cancellationToken = default) =>
        await Filtered(companyId, search)
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(Guid companyId, string? search, CancellationToken cancellationToken = default) =>
        Filtered(companyId, search).CountAsync(cancellationToken);

    private IQueryable<Contact> Filtered(Guid companyId, string? search)
    {
        var query = _context.Contacts.Where(c => c.CompanyId == companyId);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(c => EF.Functions.ILike(c.Name, pattern) || (c.Email != null && EF.Functions.ILike(c.Email, pattern)));
        }

        return query;
    }
}
