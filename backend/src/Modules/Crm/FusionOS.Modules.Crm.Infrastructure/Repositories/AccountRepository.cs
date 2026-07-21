using FusionOS.Modules.Crm.Application.Accounts.Contracts;
using FusionOS.Modules.Crm.Domain.Accounts;
using FusionOS.Modules.Crm.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Crm.Infrastructure.Repositories;

public sealed class AccountRepository : IAccountRepository
{
    private readonly CrmDbContext _context;

    public AccountRepository(CrmDbContext context) => _context = context;

    public Task<Account?> GetByIdAsync(Guid companyId, Guid accountId, CancellationToken cancellationToken = default) =>
        _context.Accounts.FirstOrDefaultAsync(a => a.CompanyId == companyId && a.Id == accountId, cancellationToken);

    public Task<bool> ExistsAsync(Guid companyId, Guid accountId, CancellationToken cancellationToken = default) =>
        _context.Accounts.AnyAsync(a => a.CompanyId == companyId && a.Id == accountId, cancellationToken);

    public async Task AddAsync(Account account, CancellationToken cancellationToken = default) =>
        await _context.Accounts.AddAsync(account, cancellationToken);

    public async Task<IReadOnlyList<Account>> ListAsync(Guid companyId, string? search, int page, int pageSize, CancellationToken cancellationToken = default) =>
        await Filtered(companyId, search)
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(Guid companyId, string? search, CancellationToken cancellationToken = default) =>
        Filtered(companyId, search).CountAsync(cancellationToken);

    private IQueryable<Account> Filtered(Guid companyId, string? search)
    {
        var query = _context.Accounts.Where(a => a.CompanyId == companyId);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(a => EF.Functions.ILike(a.Name, pattern) || (a.Industry != null && EF.Functions.ILike(a.Industry, pattern)));
        }

        return query;
    }
}
