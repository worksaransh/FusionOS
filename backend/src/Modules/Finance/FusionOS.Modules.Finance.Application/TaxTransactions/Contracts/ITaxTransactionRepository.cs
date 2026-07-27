namespace FusionOS.Modules.Finance.Application.TaxTransactions.Contracts;

public interface ITaxTransactionRepository
{
    Task AddAsync(Domain.TaxTransactions.TaxTransaction taxTransaction, CancellationToken cancellationToken = default);

    /// <summary>True if a TaxTransaction has already been recorded for this exact source line — Finance's consumers use this for idempotency, same convention as IProcessedIntegrationEventStore but scoped to the specific line rather than the whole event (an Invoice/PurchaseOrder event can carry multiple taxed lines).</summary>
    Task<bool> ExistsForSourceAsync(Guid companyId, string sourceEntityType, Guid sourceEntityId, Guid taxRateId, CancellationToken cancellationToken = default);

    /// <summary>Every TaxTransaction for the company whose TransactionDate falls within [periodStart, periodEnd] inclusive — the raw material GetGstReturnSummaryReportQueryHandler groups by TaxRate's GstComponent and Direction.</summary>
    Task<IReadOnlyList<Domain.TaxTransactions.TaxTransaction>> ListInPeriodAsync(Guid companyId, DateTimeOffset periodStart, DateTimeOffset periodEnd, CancellationToken cancellationToken = default);
}
