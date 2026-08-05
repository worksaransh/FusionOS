namespace FusionOS.Modules.Finance.Application.WithholdingTaxTransactions.Contracts;

public interface IWithholdingTaxTransactionRepository
{
    Task AddAsync(Domain.WithholdingTaxTransactions.WithholdingTaxTransaction transaction, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.WithholdingTaxTransactions.WithholdingTaxTransaction>> ListAsync(Guid companyId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid companyId, CancellationToken cancellationToken = default);
}
