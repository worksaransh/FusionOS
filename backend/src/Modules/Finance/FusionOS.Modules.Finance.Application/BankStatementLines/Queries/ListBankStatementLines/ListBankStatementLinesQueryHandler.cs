using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.BankStatementLines.Commands.RecordStatementLine;
using FusionOS.Modules.Finance.Application.BankStatementLines.Contracts;
using MediatR;

namespace FusionOS.Modules.Finance.Application.BankStatementLines.Queries.ListBankStatementLines;

public sealed class ListBankStatementLinesQueryHandler : IRequestHandler<ListBankStatementLinesQuery, PagedResult<BankStatementLineDto>>
{
    private readonly IBankStatementLineRepository _repository;

    public ListBankStatementLinesQueryHandler(IBankStatementLineRepository repository) => _repository = repository;

    public async Task<PagedResult<BankStatementLineDto>> Handle(ListBankStatementLinesQuery request, CancellationToken cancellationToken)
    {
        var lines = await _repository.ListByBankAccountAsync(request.CompanyId, request.BankAccountId, request.IsReconciled, request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountByBankAccountAsync(request.CompanyId, request.BankAccountId, request.IsReconciled, cancellationToken);

        var dtos = lines.Select(RecordStatementLineCommandHandler.MapToDto).ToList();

        return new PagedResult<BankStatementLineDto>(dtos, request.Page, request.PageSize, total);
    }
}
