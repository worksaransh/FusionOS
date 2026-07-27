using FusionOS.Modules.Manufacturing.Application.WorkOrders.Contracts;
using FusionOS.Modules.Manufacturing.Domain.WorkOrders;
using MediatR;

namespace FusionOS.Modules.Manufacturing.Application.WorkOrders.Queries.GetWorkInProgressSummaryReport;

public sealed class GetWorkInProgressSummaryReportQueryHandler
    : IRequestHandler<GetWorkInProgressSummaryReportQuery, WorkInProgressSummaryReportDto>
{
    // Kept as a constant rather than inlined so the same wording appears in every
    // response — see GetWorkInProgressSummaryReportQuery's doc comment for the full
    // reasoning behind this valuation's scope.
    private const string ValuationNote =
        "WIP value is material only: for each Released work order, it is the sum of " +
        "(quantity currently issued × unit cost) across that order's components. Unit " +
        "cost is whatever value was supplied on the BOM line at authoring time — it is " +
        "never looked up from Inventory automatically (see BomLineInput's doc comment) " +
        "— so a component authored with no unit cost contributes zero to this valuation " +
        "even though material has genuinely been issued for it. Quantity issued already " +
        "nets out any returns (WorkOrderComponent.Return decrements the same running " +
        "total Issue increments), so no double-counting adjustment is needed here. Labor " +
        "and overhead are not included — Manufacturing has no costing model for either today.";

    private readonly IWorkOrderRepository _workOrderRepository;

    public GetWorkInProgressSummaryReportQueryHandler(IWorkOrderRepository workOrderRepository)
    {
        _workOrderRepository = workOrderRepository;
    }

    public async Task<WorkInProgressSummaryReportDto> Handle(GetWorkInProgressSummaryReportQuery request, CancellationToken cancellationToken)
    {
        var releasedWorkOrders = await _workOrderRepository.ListByStatusAsync(
            request.CompanyId, WorkOrderStatus.Released, cancellationToken);

        var lines = releasedWorkOrders
            .Select(workOrder => new WorkInProgressLineDto(
                workOrder.Id,
                workOrder.ProductId,
                workOrder.QuantityToProduce,
                workOrder.Components.Sum(c => c.QuantityIssued * c.UnitCost)))
            .OrderBy(l => l.WorkOrderId)
            .ToList();

        var totalWipValue = lines.Sum(l => l.TotalIssuedValue);

        return new WorkInProgressSummaryReportDto(lines, totalWipValue, ValuationNote);
    }
}
