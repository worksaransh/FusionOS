using FusionOS.Modules.Finance.Application.Payables.Contracts;
using FusionOS.Modules.Finance.Application.Reports.Contracts;
using MediatR;

namespace FusionOS.Modules.Finance.Application.Reports.Queries.GetApAgingReport;

public sealed class GetApAgingReportQueryHandler : IRequestHandler<GetApAgingReportQuery, ApAgingReportDto>
{
    private readonly IApLedgerRepository _repository;

    public GetApAgingReportQueryHandler(IApLedgerRepository repository) => _repository = repository;

    public async Task<ApAgingReportDto> Handle(GetApAgingReportQuery request, CancellationToken cancellationToken)
    {
        var balances = await _repository.GetOutstandingSupplierBalancesAsync(request.CompanyId, cancellationToken);
        var now = DateTimeOffset.UtcNow;

        var lines = balances
            .Select(b =>
            {
                var daysOutstanding = Math.Max(0, (now - b.OldestChargeDate).Days);
                var bucket = daysOutstanding switch
                {
                    <= 30 => "0-30",
                    <= 60 => "31-60",
                    <= 90 => "61-90",
                    _ => "90+",
                };
                return new ApAgingLineDto(b.SupplierId, b.Balance, b.OldestChargeDate, daysOutstanding, bucket);
            })
            .OrderByDescending(l => l.DaysOutstanding)
            .ToList();

        decimal BucketTotal(string bucket) => lines.Where(l => l.Bucket == bucket).Sum(l => l.Balance);

        return new ApAgingReportDto(
            lines,
            BucketTotal("0-30"),
            BucketTotal("31-60"),
            BucketTotal("61-90"),
            BucketTotal("90+"),
            lines.Sum(l => l.Balance));
    }
}
