using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.Payables.Contracts;
using MediatR;

namespace FusionOS.Modules.Finance.Application.Payables.Queries.ListApLedgerEntries;

public sealed class ListApLedgerEntriesQueryHandler : IRequestHandler<ListApLedgerEntriesQuery, PagedResult<ApLedgerEntryDto>>
{
    private readonly IApLedgerRepository _repository;

    public ListApLedgerEntriesQueryHandler(IApLedgerRepository repository) => _repository = repository;

    public async Task<PagedResult<ApLedgerEntryDto>> Handle(ListApLedgerEntriesQuery request, CancellationToken cancellationToken)
    {
        var entries = await _repository.ListAsync(request.CompanyId, request.SupplierId, request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountAsync(request.CompanyId, request.SupplierId, cancellationToken);

        var dtos = entries
            .Select(e => new ApLedgerEntryDto(e.Id, e.SupplierId, e.PurchaseOrderId, e.Amount, e.Description, e.TransactionDate))
            .ToList();

        return new PagedResult<ApLedgerEntryDto>(dtos, request.Page, request.PageSize, total);
    }
}
