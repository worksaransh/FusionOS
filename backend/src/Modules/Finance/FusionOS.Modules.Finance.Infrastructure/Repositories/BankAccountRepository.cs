using FusionOS.Modules.Finance.Application.BankAccounts.Contracts;
using FusionOS.Modules.Finance.Domain.BankAccounts;
using FusionOS.Modules.Finance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Finance.Infrastructure.Repositories;

public sealed class BankAccountRepository : IBankAccountRepository
{
    private readonly FinanceDbContext _context;

    public BankAccountRepository(FinanceDbContext context) => _context = context;

    public Task<bool> CodeExistsAsync(Guid companyId, string code, CancellationToken cancellationToken = default) =>
        _context.BankAccounts.AnyAsync(a => a.CompanyId == companyId && a.Code == code.Trim().ToUpper(), cancellationToken);

    public Task<bool> ExistsAsync(Guid companyId, Guid bankAccountId, CancellationToken cancellationToken = default) =>
        _context.BankAccounts.AnyAsync(a => a.CompanyId == companyId && a.Id == bankAccountId, cancellationToken);

    public Task<BankAccount?> GetByIdAsync(Guid companyId, Guid bankAccountId, CancellationToken cancellationToken = default) =>
        _context.BankAccounts.FirstOrDefaultAsync(a => a.CompanyId == companyId && a.Id == bankAccountId, cancellationToken);

    public async Task AddAsync(BankAccount bankAccount, CancellationToken cancellationToken = default) =>
        await _context.BankAccounts.AddAsync(bankAccount, cancellationToken);

    public async Task<IReadOnlyList<BankAccount>> ListAsync(Guid companyId, string? search, int page, int pageSize, CancellationToken cancellationToken = default) =>
        await Filtered(companyId, search)
            .OrderBy(a => a.Code)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(Guid companyId, string? search, CancellationToken cancellationToken = default) =>
        Filtered(companyId, search).CountAsync(cancellationToken);

    private IQueryable<BankAccount> Filtered(Guid companyId, string? search)
    {
        var query = _context.BankAccounts.Where(a => a.CompanyId == companyId);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(a => EF.Functions.ILike(a.Code, pattern) || EF.Functions.ILike(a.Name, pattern));
        }
        return query;
    }
}
