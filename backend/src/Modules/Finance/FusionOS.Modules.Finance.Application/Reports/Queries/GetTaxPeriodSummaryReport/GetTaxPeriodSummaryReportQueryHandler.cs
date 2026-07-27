using FusionOS.Modules.Finance.Application.Reports.Contracts;
using FusionOS.Modules.Finance.Application.TaxJurisdictions.Contracts;
using FusionOS.Modules.Finance.Application.TaxRates.Contracts;
using MediatR;

namespace FusionOS.Modules.Finance.Application.Reports.Queries.GetTaxPeriodSummaryReport;

public sealed class GetTaxPeriodSummaryReportQueryHandler : IRequestHandler<GetTaxPeriodSummaryReportQuery, TaxPeriodSummaryReportDto>
{
    // Kept as a constant rather than inlined so the same wording appears in
    // every response — see GetTaxPeriodSummaryReportQuery's doc comment for
    // the full history behind this gap and its resolution.
    private const string DataAvailabilityNote =
        "This report lists the active tax rate register only — it does not compute period output/input tax " +
        "amounts. Real filed amounts (output tax collected, input tax credit, net payable) are now available " +
        "from GetGstReturnSummaryReportQuery, which groups Finance's TaxTransaction records (populated from " +
        "Sales' InvoiceIssued and Procurement's PurchaseOrderGoodsReceiptCosted events, both extended to " +
        "carry tax data) by GST component and direction. Use that report for GSTR-1/GSTR-3B filing; this one " +
        "remains a simple rate/jurisdiction listing.";

    private readonly ITaxRateRepository _taxRateRepository;
    private readonly ITaxJurisdictionRepository _taxJurisdictionRepository;

    public GetTaxPeriodSummaryReportQueryHandler(ITaxRateRepository taxRateRepository, ITaxJurisdictionRepository taxJurisdictionRepository)
    {
        _taxRateRepository = taxRateRepository;
        _taxJurisdictionRepository = taxJurisdictionRepository;
    }

    public async Task<TaxPeriodSummaryReportDto> Handle(GetTaxPeriodSummaryReportQuery request, CancellationToken cancellationToken)
    {
        var jurisdictions = (await _taxJurisdictionRepository.ListAllActiveAsync(request.CompanyId, cancellationToken))
            .ToDictionary(j => j.Id);
        var rates = await _taxRateRepository.ListAllActiveAsync(request.CompanyId, cancellationToken);

        var lines = new List<TaxPeriodSummaryLineDto>();
        foreach (var rate in rates)
        {
            // A rate whose parent jurisdiction has itself been deactivated doesn't
            // belong on an active filing register — same "look up the parent,
            // skip orphans" caution as GetTrialBalanceQueryHandler takes with
            // accounts, except here a missing/inactive jurisdiction is an expected
            // state transition (Deactivate()), not a data-integrity error, so this
            // skips rather than throws.
            if (!jurisdictions.TryGetValue(rate.TaxJurisdictionId, out var jurisdiction))
                continue;

            lines.Add(new TaxPeriodSummaryLineDto(
                jurisdiction.Id,
                jurisdiction.Code,
                jurisdiction.Name,
                rate.Id,
                rate.Code,
                rate.Name,
                rate.Percentage,
                OutputTaxCollected: null,
                InputTaxPaid: null,
                NetTaxPayable: null));
        }

        var ordered = lines
            .OrderBy(l => l.TaxJurisdictionCode, StringComparer.Ordinal)
            .ThenBy(l => l.TaxRateCode, StringComparer.Ordinal)
            .ToList();

        return new TaxPeriodSummaryReportDto(
            request.PeriodStart,
            request.PeriodEnd,
            ordered,
            TotalOutputTaxCollected: null,
            TotalInputTaxPaid: null,
            TotalNetTaxPayable: null,
            DataAvailabilityNote);
    }
}
