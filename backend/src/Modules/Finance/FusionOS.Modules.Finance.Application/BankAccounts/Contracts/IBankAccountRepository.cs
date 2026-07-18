namespace FusionOS.Modules.Finance.Application.BankAccounts.Contracts;

/// <summary>
/// Mirrors ICostCenterRepository's shape exactly, plus ExistsAsync — used by
/// RecordStatementLineCommandHandler to verify a BankStatementLine's
/// BankAccountId actually exists before recording a line against it, same
/// "handler checks cross-aggregate existence" split
/// CreateTaxRateCommandHandler uses via
/// ITaxRateRepository.TaxJurisdictionExistsAsync.
/// </summary>
public interface IBankAccountRepository
{
    Task<bool> CodeExistsAsync(Guid companyId, string code, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid companyId, Guid bankAccountId, CancellationToken cancellationToken = default);
    Task<Domain.BankAccounts.BankAccount?> GetByIdAsync(Guid companyId, Guid bankAccountId, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.BankAccounts.BankAccount bankAccount, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.BankAccounts.BankAccount>> ListAsync(Guid companyId, string? search, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid companyId, string? search, CancellationToken cancellationToken = default);
}
