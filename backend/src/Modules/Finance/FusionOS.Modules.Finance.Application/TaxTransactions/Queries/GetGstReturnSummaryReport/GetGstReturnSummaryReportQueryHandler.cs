using FusionOS.Modules.Finance.Application.TaxRates.Contracts;
using FusionOS.Modules.Finance.Application.TaxTransactions.Contracts;
using FusionOS.Modules.Finance.Domain.TaxRates;
using FusionOS.Modules.Finance.Domain.TaxTransactions;
using MediatR;

namespace FusionOS.Modules.Finance.Application.TaxTransactions.Queries.GetGstReturnSummaryReport;

public sealed class GetGstReturnSummaryReportQueryHandler : IRequestHandler<GetGstReturnSummaryReportQuery, GstReturnSummaryReportDto>
{
    private readonly ITaxTransactionRepository _taxTransactionRepository;
    private readonly ITaxRateRepository _taxRateRepository;

    public GetGstReturnSummaryReportQueryHandler(ITaxTransactionRepository taxTransactionRepository, ITaxRateRepository taxRateRepository)
    {
        _taxTransactionRepository = taxTransactionRepository;
        _taxRateRepository = taxRateRepository;
    }

    public async Task<GstReturnSummaryReportDto> Handle(GetGstReturnSummaryReportQuery request, CancellationToken cancellationToken)
    {
        var transactions = await _taxTransactionRepository.ListInPeriodAsync(request.CompanyId, request.PeriodStart, request.PeriodEnd, cancellationToken);

        // Batch-resolve every distinct TaxRateId referenced in the period in one round trip
        // instead of one GetByIdAsync call per distinct rate (was N+1 for periods spanning
        // many rates) — same "aggregate in the database, resolve related data in bulk"
        // convention as this report's totals.
        var distinctRateIds = transactions.Select(t => t.TaxRateId).Distinct().ToList();
        var rates = await _taxRateRepository.GetByIdsAsync(request.CompanyId, distinctRateIds, cancellationToken);
        var componentByRateId = rates.ToDictionary(r => r.Id, r => r.GstComponent);

        var byComponent = new Dictionary<GstComponent, (decimal Output, decimal Input)>();

        foreach (var transaction in transactions)
        {
            // A TaxRate deleted or moved out of this company after the transaction was recorded is a data-integrity
            // anomaly, not an expected state transition (TaxRate only ever deactivates, never deletes) — skip rather
            // than throw, since a filing report should still summarize whatever rates it CAN resolve.
            if (!componentByRateId.TryGetValue(transaction.TaxRateId, out var component))
                continue;

            var (output, input) = byComponent.TryGetValue(component, out var totals) ? totals : (0m, 0m);
            byComponent[component] = transaction.Direction == TaxTransactionDirection.Output
                ? (output + transaction.TaxAmount, input)
                : (output, input + transaction.TaxAmount);
        }

        var lines = byComponent
            .Select(kvp => new GstReturnComponentLineDto(kvp.Key.ToString(), kvp.Value.Output, kvp.Value.Input, kvp.Value.Output - kvp.Value.Input))
            .OrderBy(l => l.GstComponent, StringComparer.Ordinal)
            .ToList();

        var totalOutput = lines.Sum(l => l.OutputTaxCollected);
        var totalInput = lines.Sum(l => l.InputTaxCredit);

        return new GstReturnSummaryReportDto(
            request.PeriodStart,
            request.PeriodEnd,
            lines,
            totalOutput,
            totalInput,
            totalOutput - totalInput);
    }
}
