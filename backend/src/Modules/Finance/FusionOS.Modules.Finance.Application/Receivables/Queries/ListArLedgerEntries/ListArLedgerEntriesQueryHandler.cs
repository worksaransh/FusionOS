using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.Receivables.Contracts;
using MediatR;

namespace FusionOS.Modules.Finance.Application.Receivables.Queries.ListArLedgerEntries;

public sealed class ListArLedgerEntriesQueryHandler : IRequestHandler<ListArLedgerEntriesQuery, PagedResult<ArLedgerEntryDto>>
{
    private readonly IArLedgerRepository _repository;

    public ListArLedgerEntriesQueryHandler(IArLedgerRepository repository) => _repository = repository;

    public async Task<PagedResult<ArLedgerEntryDto>> Handle(ListArLedgerEntriesQuery request, CancellationToken cancellationToken)
    {
        var entries = await _repository.ListAsync(request.CompanyId, request.CustomerId, request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountAsync(request.CompanyId, request.CustomerId, cancellationToken);

        var dtos = entries
            .Select(e => new ArLedgerEntryDto(e.Id, e.CustomerId, e.InvoiceId, e.Amount, e.Description, e.TransactionDate))
            .ToList();

        return new PagedResult<ArLedgerEntryDto>(dtos, request.Page, request.PageSize, total);
    }
}
