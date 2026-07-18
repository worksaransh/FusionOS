namespace FusionOS.Modules.Finance.Application.Accounts.Contracts;

public interface IAccountRepository
{
    Task<bool> CodeExistsAsync(Guid companyId, string code, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid companyId, Guid accountId, CancellationToken cancellationToken = default);
    Task<Domain.Accounts.Account?> GetByIdAsync(Guid companyId, Guid accountId, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.Accounts.Account account, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.Accounts.Account>> ListAsync(Guid companyId, string? search, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid companyId, string? search, CancellationToken cancellationToken = default);

    /// <summary>Every account for this company, unpaged — backs the P&amp;L/Balance Sheet reports (Phase 2 closeout, 2026-07-18), which need the full Chart of Accounts to join against posted balances, not a page of it.</summary>
    Task<IReadOnlyList<Domain.Accounts.Account>> ListAllAsync(Guid companyId, CancellationToken cancellationToken = default);
}
