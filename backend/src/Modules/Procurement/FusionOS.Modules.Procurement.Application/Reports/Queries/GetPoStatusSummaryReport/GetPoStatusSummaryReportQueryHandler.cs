using FusionOS.Modules.Procurement.Application.PurchaseOrders.Contracts;
using FusionOS.Modules.Procurement.Application.Reports.Contracts;
using FusionOS.Modules.Procurement.Domain.PurchaseOrders;
using MediatR;

namespace FusionOS.Modules.Procurement.Application.Reports.Queries.GetPoStatusSummaryReport;

public sealed class GetPoStatusSummaryReportQueryHandler : IRequestHandler<GetPoStatusSummaryReportQuery, PoStatusSummaryReportDto>
{
    private readonly IPurchaseOrderRepository _repository;

    public GetPoStatusSummaryReportQueryHandler(IPurchaseOrderRepository repository) => _repository = repository;

    public async Task<PoStatusSummaryReportDto> Handle(GetPoStatusSummaryReportQuery request, CancellationToken cancellationToken)
    {
        var counts = await _repository.CountByStatusAsync(request.CompanyId, cancellationToken);
        var lookup = counts.ToDictionary(c => c.Status, c => c.Count);

        // Every known status is represented, even at zero — a dashboard widget
        // shouldn't have to guess whether "Approved: 0" means zero orders or a
        // status this report forgot to ask about.
        var lines = Enum.GetValues<PurchaseOrderStatus>()
            .Select(status => new PoStatusSummaryLineDto(status.ToString(), lookup.GetValueOrDefault(status)))
            .ToList();

        return new PoStatusSummaryReportDto(lines, lines.Sum(l => l.Count));
    }
}
