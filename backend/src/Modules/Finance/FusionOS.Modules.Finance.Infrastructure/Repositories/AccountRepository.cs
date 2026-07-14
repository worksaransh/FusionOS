using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Domain.Accounts;
using FusionOS.Modules.Finance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Finance.Infrastructure.Repositories;

public sealed class AccountRepository : IAccountRepository
{
    private readonly FinanceDbContext _context;

    public AccountRepository(FinanceDbContext context) => _context = context;

    public Task<bool> CodeExistsAsync(Guid companyId, string code, CancellationToken cancellationToken = default) =>
        _context.Accounts.AnyAsync(a => a.CompanyId == companyId && a.Code == code.Trim().ToUpper(), cancellationToken);

    public Task<bool> ExistsAsync(Guid companyId, Guid accountId, CancellationToken cancellationToken = default) =>
        _context.Accounts.AnyAsync(a => a.CompanyId == companyId && a.Id == accountId, cancellationToken);

    public async Task AddAsync(Account account, CancellationToken cancellationToken = default) =>
        await _context.Accounts.AddAsync(account, cancellationToken);

    public async Task<IReadOnlyList<Account>> ListAsync(Guid companyId, string? search, int page, int pageSize, CancellationToken cancellationToken = default) =>
        await Filtered(companyId, search)
            .OrderBy(a => a.Code)
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
            query = query.Where(a => EF.Functions.ILike(a.Code, pattern) || EF.Functions.ILike(a.Name, pattern));
        }
        return query;
    }
}
