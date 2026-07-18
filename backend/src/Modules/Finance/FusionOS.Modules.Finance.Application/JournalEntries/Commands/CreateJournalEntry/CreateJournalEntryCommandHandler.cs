using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.CostCenters.Contracts;
using FusionOS.Modules.Finance.Application.JournalEntries.Contracts;
using MediatR;

namespace FusionOS.Modules.Finance.Application.JournalEntries.Commands.CreateJournalEntry;

public sealed class CreateJournalEntryCommandHandler : IRequestHandler<CreateJournalEntryCommand, JournalEntryDto>
{
    private readonly IJournalEntryRepository _repository;
    private readonly IAccountRepository _accountRepository;
    private readonly ICostCenterRepository _costCenterRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateJournalEntryCommandHandler(IJournalEntryRepository repository, IAccountRepository accountRepository, ICostCenterRepository costCenterRepository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _accountRepository = accountRepository;
        _costCenterRepository = costCenterRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<JournalEntryDto> Handle(CreateJournalEntryCommand request, CancellationToken cancellationToken)
    {
        foreach (var line in request.Lines)
        {
            if (!await _accountRepository.ExistsAsync(request.CompanyId, line.AccountId, cancellationToken))
            {
                throw new ValidationException(new[]
                {
                    new FluentValidation.Results.ValidationFailure(nameof(line.AccountId), $"Account '{line.AccountId}' does not exist for this company."),
                });
            }

            if (line.CostCenterId is { } costCenterId
                && await _costCenterRepository.GetByIdAsync(request.CompanyId, costCenterId, cancellationToken) is null)
            {
                throw new ValidationException(new[]
                {
                    new FluentValidation.Results.ValidationFailure(nameof(line.CostCenterId), $"Cost center '{costCenterId}' does not exist for this company."),
                });
            }
        }

        var entry = Domain.JournalEntries.JournalEntry.Create(request.CompanyId, request.Reference, request.Lines);

        await _repository.AddAsync(entry, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(entry);
    }

    internal static JournalEntryDto MapToDto(Domain.JournalEntries.JournalEntry entry) => new(
        entry.Id,
        entry.Reference,
        entry.Status.ToString(),
        entry.EntryDate,
        entry.TotalDebit,
        entry.TotalCredit,
        entry.Lines.Select(l => new JournalEntryLineDto(l.Id, l.AccountId, l.Debit, l.Credit, l.Description, l.CostCenterId)).ToList());
}
