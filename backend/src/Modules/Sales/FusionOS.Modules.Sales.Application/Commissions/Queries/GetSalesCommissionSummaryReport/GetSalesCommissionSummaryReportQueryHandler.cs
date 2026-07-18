using FusionOS.Modules.Sales.Application.Commissions.Contracts;
using FusionOS.Modules.Sales.Application.Invoices.Contracts;
using MediatR;

namespace FusionOS.Modules.Sales.Application.Commissions.Queries.GetSalesCommissionSummaryReport;

public sealed class GetSalesCommissionSummaryReportQueryHandler : IRequestHandler<GetSalesCommissionSummaryReportQuery, IReadOnlyList<SalesCommissionSummaryLineDto>>
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly ISalesCommissionRateRepository _rateRepository;

    public GetSalesCommissionSummaryReportQueryHandler(IInvoiceRepository invoiceRepository, ISalesCommissionRateRepository rateRepository)
    {
        _invoiceRepository = invoiceRepository;
        _rateRepository = rateRepository;
    }

    public async Task<IReadOnlyList<SalesCommissionSummaryLineDto>> Handle(GetSalesCommissionSummaryReportQuery request, CancellationToken cancellationToken)
    {
        var totals = await _invoiceRepository.GetIssuedInvoiceTotalsBySalesPersonAsync(request.CompanyId, cancellationToken);

        var lines = new List<SalesCommissionSummaryLineDto>();
        foreach (var (salesPersonId, totalInvoicedRevenue) in totals)
        {
            // Defaults to a 0% rate if the salesperson has never had one set —
            // same "default doesn't block the read" restraint as CompanySettings'
            // get-or-create pattern, just without the create half since this is
            // a read-only report.
            var rate = await _rateRepository.GetByUserIdAsync(request.CompanyId, salesPersonId, cancellationToken);
            var ratePercentage = rate?.RatePercentage ?? 0m;

            lines.Add(new SalesCommissionSummaryLineDto(
                salesPersonId,
                totalInvoicedRevenue,
                ratePercentage,
                totalInvoicedRevenue * ratePercentage / 100m));
        }

        return lines;
    }
}
