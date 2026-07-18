using FusionOS.Modules.Inventory.Application.Ledger.Contracts;
using FusionOS.Modules.Inventory.Application.Reports.Contracts;
using MediatR;

namespace FusionOS.Modules.Inventory.Application.Reports.Queries.GetStockValuationReport;

public sealed class GetStockValuationReportQueryHandler : IRequestHandler<GetStockValuationReportQuery, StockValuationReportDto>
{
    private readonly IInventoryLedgerRepository _repository;

    public GetStockValuationReportQueryHandler(IInventoryLedgerRepository repository) => _repository = repository;

    public async Task<StockValuationReportDto> Handle(GetStockValuationReportQuery request, CancellationToken cancellationToken)
    {
        var rows = await _repository.GetStockValuationAsync(request.CompanyId, cancellationToken);

        var lines = rows
            .Select(r => new StockValuationLineDto(r.ProductId, r.Sku, r.Name, r.OnHandQuantity, r.LastUnitCost, r.OnHandQuantity * (r.LastUnitCost ?? 0m)))
            .OrderBy(l => l.Sku)
            .ToList();

        return new StockValuationReportDto(lines, lines.Sum(l => l.ExtendedValue));
    }
}
