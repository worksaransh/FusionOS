using FusionOS.Modules.Inventory.Application.Ledger.Contracts;
using FusionOS.Modules.Inventory.Application.Reports.Contracts;
using FusionOS.Modules.Inventory.Domain.Costing;
using MediatR;

namespace FusionOS.Modules.Inventory.Application.Reports.Queries.GetInventoryValuationReport;

public sealed class GetInventoryValuationReportQueryHandler : IRequestHandler<GetInventoryValuationReportQuery, InventoryValuationReportDto>
{
    private readonly IInventoryLedgerRepository _repository;

    public GetInventoryValuationReportQueryHandler(IInventoryLedgerRepository repository) => _repository = repository;

    public async Task<InventoryValuationReportDto> Handle(GetInventoryValuationReportQuery request, CancellationToken cancellationToken)
    {
        var rows = await _repository.GetLedgerEntriesByProductAsync(request.CompanyId, cancellationToken);

        var lines = rows
            .Select(r =>
            {
                var snapshot = WeightedAverageCostCalculator.Calculate(r.Entries);
                return new InventoryValuationLineDto(
                    r.ProductId,
                    r.Sku,
                    r.Name,
                    snapshot.OnHandQuantity,
                    snapshot.WeightedAverageUnitCost,
                    snapshot.TotalValuation,
                    snapshot.CumulativeCostOfGoodsSold);
            })
            .OrderBy(l => l.Sku)
            .ToList();

        return new InventoryValuationReportDto(
            lines,
            lines.Sum(l => l.TotalValuation),
            lines.Sum(l => l.CumulativeCostOfGoodsSold));
    }
}
