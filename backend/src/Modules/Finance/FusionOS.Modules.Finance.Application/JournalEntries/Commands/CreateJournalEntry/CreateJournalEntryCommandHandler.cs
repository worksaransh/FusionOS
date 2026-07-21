using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.CostCenters.Contracts;
using FusionOS.Modules.Finance.Application.JournalEntries.Contracts;
using FusionOS.Modules.Finance.Domain.JournalEntries;
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
        // Lines are grouped by AccountId (and separately by CostCenterId) before
        // checking, so the same account/cost center referenced by several request
        // lines is only looked up once instead of once per line.
        foreach (var accountLines in request.Lines.GroupBy(l => l.AccountId))
        {
            if (!await _accountRepository.ExistsAsync(request.CompanyId, accountLines.Key, cancellationToken))
            {
                throw new ValidationException(new[]
                {
                    new FluentValidation.Results.ValidationFailure(nameof(JournalEntryLineInput.AccountId), $"Account '{accountLines.Key}' does not exist for this company."),
                });
            }
        }

        foreach (var costCenterLines in request.Lines.Where(l => l.CostCenterId is not null).GroupBy(l => l.CostCenterId!.Value))
        {
            if (await _costCenterRepository.GetByIdAsync(request.CompanyId, costCenterLines.Key, cancellationToken) is null)
            {
                throw new ValidationException(new[]
                {
                    new FluentValidation.Results.ValidationFailure(nameof(JournalEntryLineInput.CostCenterId), $"Cost center '{costCenterLines.Key}' does not exist for this company."),
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
