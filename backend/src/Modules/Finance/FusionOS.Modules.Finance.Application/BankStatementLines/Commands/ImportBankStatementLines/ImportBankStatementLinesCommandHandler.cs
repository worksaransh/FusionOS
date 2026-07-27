using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.BankAccounts.Contracts;
using FusionOS.Modules.Finance.Application.BankStatementLines.Contracts;
using FusionOS.Modules.Finance.Domain.BankStatementLines;
using MediatR;

namespace FusionOS.Modules.Finance.Application.BankStatementLines.Commands.ImportBankStatementLines;

/// <summary>
/// Persists the batch of already-parsed rows ImportBankStatementLinesCommand
/// carries. BankAccountId existence is checked once up front, same as
/// RecordStatementLineCommandHandler — an import against a bank account that
/// doesn't exist fails the whole request, since there is nothing meaningful
/// to import against. Per-row problems (a malformed amount that fails
/// BankStatementLine.Create's own guards, or a row that's already been
/// imported before) are collected and reported instead of aborting the rest
/// of the batch — the "skip one bad apple, keep going" behavior called out
/// in the class doc comment on ImportBankStatementLinesCommand.
/// </summary>
public sealed class ImportBankStatementLinesCommandHandler : IRequestHandler<ImportBankStatementLinesCommand, BankStatementLineImportResultDto>
{
    private readonly IBankStatementLineRepository _repository;
    private readonly IBankAccountRepository _bankAccountRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ImportBankStatementLinesCommandHandler(IBankStatementLineRepository repository, IBankAccountRepository bankAccountRepository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _bankAccountRepository = bankAccountRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<BankStatementLineImportResultDto> Handle(ImportBankStatementLinesCommand request, CancellationToken cancellationToken)
    {
        if (!await _bankAccountRepository.ExistsAsync(request.CompanyId, request.BankAccountId, cancellationToken))
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.BankAccountId), "Bank account does not exist for this company."),
            });
        }

        var imported = 0;
        var skippedAsDuplicate = 0;
        var errors = new List<ImportBankStatementLineRowErrorDto>();

        foreach (var line in request.Lines)
        {
            var description = line.Description?.Trim() ?? string.Empty;

            try
            {
                if (await _repository.ExistsDuplicateAsync(request.CompanyId, request.BankAccountId, line.TransactionDate, line.Amount, description, cancellationToken))
                {
                    skippedAsDuplicate++;
                    continue;
                }

                var statementLine = BankStatementLine.Create(request.CompanyId, request.BankAccountId, line.TransactionDate, line.Amount, description);
                await _repository.AddAsync(statementLine, cancellationToken);
                imported++;
            }
            catch (ArgumentException ex)
            {
                errors.Add(new ImportBankStatementLineRowErrorDto(line.RowNumber, ex.Message));
            }
        }

        if (imported > 0)
            await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new BankStatementLineImportResultDto(request.Lines.Count, imported, skippedAsDuplicate, errors);
    }
}
