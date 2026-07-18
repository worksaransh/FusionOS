using FusionOS.Modules.Procurement.Application.PurchaseOrders.Contracts;
using FusionOS.Modules.Procurement.Application.Reports.Contracts;
using MediatR;

namespace FusionOS.Modules.Procurement.Application.Reports.Queries.GetPriceHistoryReport;

public sealed class GetPriceHistoryReportQueryHandler : IRequestHandler<GetPriceHistoryReportQuery, IReadOnlyList<PriceHistoryLineDto>>
{
    private readonly IPurchaseOrderRepository _repository;

    public GetPriceHistoryReportQueryHandler(IPurchaseOrderRepository repository) => _repository = repository;

    public async Task<IReadOnlyList<PriceHistoryLineDto>> Handle(GetPriceHistoryReportQuery request, CancellationToken cancellationToken)
    {
        var history = await _repository.GetPriceHistoryAsync(request.CompanyId, request.ProductId, cancellationToken);

        return history
            .Select(h => new PriceHistoryLineDto(h.PurchaseOrderId, h.SupplierId, h.OrderDate, h.UnitPrice, h.Quantity))
            .ToList();
    }
}
