using FusionOS.Modules.Finance.Application.Receivables.Contracts;
using FusionOS.Modules.Finance.Application.Reports.Contracts;
using MediatR;

namespace FusionOS.Modules.Finance.Application.Reports.Queries.GetArAgingReport;

public sealed class GetArAgingReportQueryHandler : IRequestHandler<GetArAgingReportQuery, ArAgingReportDto>
{
    private readonly IArLedgerRepository _repository;

    public GetArAgingReportQueryHandler(IArLedgerRepository repository) => _repository = repository;

    public async Task<ArAgingReportDto> Handle(GetArAgingReportQuery request, CancellationToken cancellationToken)
    {
        var balances = await _repository.GetOutstandingInvoiceBalancesAsync(request.CompanyId, cancellationToken);
        var now = DateTimeOffset.UtcNow;

        var lines = balances
            .Select(b =>
            {
                var daysOutstanding = Math.Max(0, (now - b.ChargeDate).Days);
                var bucket = daysOutstanding switch
                {
                    <= 30 => "0-30",
                    <= 60 => "31-60",
                    <= 90 => "61-90",
                    _ => "90+",
                };
                return new ArAgingLineDto(b.CustomerId, b.InvoiceId, b.Balance, b.ChargeDate, daysOutstanding, bucket);
            })
            .OrderByDescending(l => l.DaysOutstanding)
            .ToList();

        decimal BucketTotal(string bucket) => lines.Where(l => l.Bucket == bucket).Sum(l => l.Balance);

        return new ArAgingReportDto(
            lines,
            BucketTotal("0-30"),
            BucketTotal("31-60"),
            BucketTotal("61-90"),
            BucketTotal("90+"),
            lines.Sum(l => l.Balance));
    }
}
