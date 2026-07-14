namespace FusionOS.Modules.Finance.Application.Receivables.Contracts;

public interface IArLedgerRepository
{
    Task AddAsync(Domain.Receivables.ArLedgerEntry entry, CancellationToken cancellationToken = default);

    Task<decimal> SumAmountAsync(Guid companyId, Guid customerId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Domain.Receivables.ArLedgerEntry>> ListAsync(Guid companyId, Guid customerId, int page, int pageSize, CancellationToken cancellationToken = default);

    Task<int> CountAsync(Guid companyId, Guid customerId, CancellationToken cancellationToken = default);
}
