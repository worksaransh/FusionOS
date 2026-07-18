using FusionOS.Modules.Finance.Application.BankStatementLines.Contracts;
using MediatR;

namespace FusionOS.Modules.Finance.Application.BankStatementLines.Queries.GetReconciliationSummary;

public sealed class GetReconciliationSummaryQueryHandler : IRequestHandler<GetReconciliationSummaryQuery, ReconciliationSummaryDto>
{
    private readonly IBankStatementLineRepository _repository;

    public GetReconciliationSummaryQueryHandler(IBankStatementLineRepository repository) => _repository = repository;

    public async Task<ReconciliationSummaryDto> Handle(GetReconciliationSummaryQuery request, CancellationToken cancellationToken)
    {
        var (totalLines, reconciledCount, unreconciledCount, unreconciledTotalAmount) =
            await _repository.GetReconciliationSummaryAsync(request.CompanyId, request.BankAccountId, cancellationToken);

        return new ReconciliationSummaryDto(request.BankAccountId, totalLines, reconciledCount, unreconciledCount, unreconciledTotalAmount);
    }
}
