using FusionOS.Modules.Procurement.Application.PurchaseOrders.Contracts;
using FusionOS.Modules.Procurement.Application.Reports.Contracts;
using MediatR;

namespace FusionOS.Modules.Procurement.Application.Reports.Queries.GetSupplierScorecardReport;

public sealed class GetSupplierScorecardReportQueryHandler : IRequestHandler<GetSupplierScorecardReportQuery, IReadOnlyList<SupplierScorecardLineDto>>
{
    private readonly IPurchaseOrderRepository _repository;

    public GetSupplierScorecardReportQueryHandler(IPurchaseOrderRepository repository) => _repository = repository;

    public async Task<IReadOnlyList<SupplierScorecardLineDto>> Handle(GetSupplierScorecardReportQuery request, CancellationToken cancellationToken)
    {
        var stats = await _repository.GetSupplierOrderStatsAsync(request.CompanyId, cancellationToken);

        return stats
            .Select(s => new SupplierScorecardLineDto(
                s.SupplierId,
                s.OrderCount,
                s.TotalOrderValue,
                s.OrderCount > 0 ? s.TotalOrderValue / s.OrderCount : 0m,
                s.FullyReceivedCount,
                s.OrderCount > 0 ? (decimal)s.FullyReceivedCount / s.OrderCount * 100m : 0m))
            .ToList();
    }
}
