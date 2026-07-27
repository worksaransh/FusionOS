namespace FusionOS.Modules.Crm.Application.Accounts.Contracts;

public interface IAccountRepository
{
    Task<Domain.Accounts.Account?> GetByIdAsync(Guid companyId, Guid accountId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid companyId, Guid accountId, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.Accounts.Account account, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.Accounts.Account>> ListAsync(Guid companyId, string? search, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid companyId, string? search, CancellationToken cancellationToken = default);

    /// <summary>
    /// Advisory duplicate check for CreateAccountCommandHandler — does any active Account in
    /// this company already carry this exact normalized (trimmed, lower-cased) name? Account
    /// has no Email field (see Account.cs), so Name is the closest available identity
    /// signal. Used only to populate AccountDto.IsPossibleDuplicate as a warning, never to
    /// block creation.
    /// </summary>
    Task<bool> IsNameAlreadyUsedAsync(Guid companyId, string normalizedName, CancellationToken cancellationToken = default);
}
