using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.BankStatementLines.Commands.RecordStatementLine;
using FusionOS.Modules.Finance.Application.BankStatementLines.Contracts;
using MediatR;

namespace FusionOS.Modules.Finance.Application.BankStatementLines.Commands.ReconcileStatementLine;

public sealed class ReconcileStatementLineCommandHandler : IRequestHandler<ReconcileStatementLineCommand, BankStatementLineDto>
{
    private readonly IBankStatementLineRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public ReconcileStatementLineCommandHandler(IBankStatementLineRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<BankStatementLineDto> Handle(ReconcileStatementLineCommand request, CancellationToken cancellationToken)
    {
        var line = await _repository.GetByIdAsync(request.CompanyId, request.StatementLineId, cancellationToken)
            ?? throw new KeyNotFoundException($"Bank statement line '{request.StatementLineId}' was not found.");

        line.Reconcile(request.MatchedJournalEntryId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return RecordStatementLineCommandHandler.MapToDto(line);
    }
}
