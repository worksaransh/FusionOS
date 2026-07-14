namespace FusionOS.Modules.Finance.Application.Accounts.Contracts;

public interface IAccountRepository
{
    Task<bool> CodeExistsAsync(Guid companyId, string code, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid companyId, Guid accountId, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.Accounts.Account account, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.Accounts.Account>> ListAsync(Guid companyId, string? search, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid companyId, string? search, CancellationToken cancellationToken = default);
}
