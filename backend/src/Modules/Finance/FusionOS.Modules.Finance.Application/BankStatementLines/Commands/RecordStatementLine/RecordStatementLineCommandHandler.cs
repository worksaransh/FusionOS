using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.BankAccounts.Contracts;
using FusionOS.Modules.Finance.Application.BankStatementLines.Contracts;
using FusionOS.Modules.Finance.Domain.BankStatementLines;
using MediatR;

namespace FusionOS.Modules.Finance.Application.BankStatementLines.Commands.RecordStatementLine;

public sealed class RecordStatementLineCommandHandler : IRequestHandler<RecordStatementLineCommand, BankStatementLineDto>
{
    private readonly IBankStatementLineRepository _repository;
    private readonly IBankAccountRepository _bankAccountRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RecordStatementLineCommandHandler(IBankStatementLineRepository repository, IBankAccountRepository bankAccountRepository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _bankAccountRepository = bankAccountRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<BankStatementLineDto> Handle(RecordStatementLineCommand request, CancellationToken cancellationToken)
    {
        if (!await _bankAccountRepository.ExistsAsync(request.CompanyId, request.BankAccountId, cancellationToken))
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.BankAccountId), "Bank account does not exist for this company."),
            });
        }

        var line = BankStatementLine.Create(request.CompanyId, request.BankAccountId, request.TransactionDate, request.Amount, request.Description);

        await _repository.AddAsync(line, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(line);
    }

    internal static BankStatementLineDto MapToDto(BankStatementLine line) => new(
        line.Id, line.BankAccountId, line.TransactionDate, line.Amount, line.Description,
        line.IsReconciled, line.ReconciledAt, line.MatchedJournalEntryId);
}
