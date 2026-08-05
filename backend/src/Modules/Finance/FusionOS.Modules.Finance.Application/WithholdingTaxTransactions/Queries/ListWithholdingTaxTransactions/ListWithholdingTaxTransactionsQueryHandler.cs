using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.WithholdingTaxTransactions.Contracts;
using MediatR;

namespace FusionOS.Modules.Finance.Application.WithholdingTaxTransactions.Queries.ListWithholdingTaxTransactions;

public sealed class ListWithholdingTaxTransactionsQueryHandler : IRequestHandler<ListWithholdingTaxTransactionsQuery, PagedResult<WithholdingTaxTransactionDto>>
{
    private readonly IWithholdingTaxTransactionRepository _repository;

    public ListWithholdingTaxTransactionsQueryHandler(IWithholdingTaxTransactionRepository repository) => _repository = repository;

    public async Task<PagedResult<WithholdingTaxTransactionDto>> Handle(ListWithholdingTaxTransactionsQuery request, CancellationToken cancellationToken)
    {
        var transactions = await _repository.ListAsync(request.CompanyId, request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountAsync(request.CompanyId, cancellationToken);

        return new PagedResult<WithholdingTaxTransactionDto>(
            transactions.Select(t => new WithholdingTaxTransactionDto(t.Id, t.WithholdingTaxRateId, t.ReferenceType, t.ReferenceId, t.GrossAmount, t.DeductedAmount, t.TransactionDate)).ToList(),
            request.Page,
            request.PageSize,
            total);
    }
}
