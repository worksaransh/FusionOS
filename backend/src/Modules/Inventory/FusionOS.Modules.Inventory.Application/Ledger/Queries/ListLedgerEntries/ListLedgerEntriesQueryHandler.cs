using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Inventory.Application.Ledger.Contracts;
using MediatR;

namespace FusionOS.Modules.Inventory.Application.Ledger.Queries.ListLedgerEntries;

public sealed class ListLedgerEntriesQueryHandler : IRequestHandler<ListLedgerEntriesQuery, PagedResult<InventoryLedgerEntryDto>>
{
    private readonly IInventoryLedgerRepository _repository;

    public ListLedgerEntriesQueryHandler(IInventoryLedgerRepository repository) => _repository = repository;

    public async Task<PagedResult<InventoryLedgerEntryDto>> Handle(ListLedgerEntriesQuery request, CancellationToken cancellationToken)
    {
        var entries = await _repository.ListAsync(request.CompanyId, request.ProductId, request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountAsync(request.CompanyId, request.ProductId, cancellationToken);

        var dtos = entries
            .Select(e => new InventoryLedgerEntryDto(e.Id, e.ProductId, e.WarehouseId, e.QuantityDelta, e.UnitCost, e.Reason, e.TransactionDate))
            .ToList();

        return new PagedResult<InventoryLedgerEntryDto>(dtos, request.Page, request.PageSize, total);
    }
}
