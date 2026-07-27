using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.AccountingPeriods.Contracts;
using FusionOS.Modules.Finance.Application.JournalEntries.Commands.CreateJournalEntry;
using FusionOS.Modules.Finance.Application.JournalEntries.Contracts;
using MediatR;

namespace FusionOS.Modules.Finance.Application.JournalEntries.Commands.PostJournalEntry;

public sealed class PostJournalEntryCommandHandler : IRequestHandler<PostJournalEntryCommand, JournalEntryDto>
{
    private readonly IJournalEntryRepository _repository;
    private readonly IAccountingPeriodRepository _accountingPeriodRepository;
    private readonly IUnitOfWork _unitOfWork;

    public PostJournalEntryCommandHandler(
        IJournalEntryRepository repository,
        IAccountingPeriodRepository accountingPeriodRepository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _accountingPeriodRepository = accountingPeriodRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<JournalEntryDto> Handle(PostJournalEntryCommand request, CancellationToken cancellationToken)
    {
        var entry = await _repository.GetByIdAsync(request.CompanyId, request.JournalEntryId, cancellationToken)
            ?? throw new KeyNotFoundException($"Journal entry '{request.JournalEntryId}' was not found.");

        // Period-control guard (see CreateJournalEntryCommandHandler for the create-time check).
        // The period covering this entry's EntryDate may have been closed after the entry was
        // created as a Draft, so it must be re-checked here at post time — otherwise a Draft
        // created while its period was open could still be posted into an already-closed period.
        var closedPeriod = await _accountingPeriodRepository.FindClosedPeriodCoveringDateAsync(request.CompanyId, entry.EntryDate, cancellationToken);
        if (closedPeriod is not null)
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(Domain.JournalEntries.JournalEntry.EntryDate),
                    $"Cannot post a journal entry: the accounting period {closedPeriod.PeriodStart:yyyy-MM-dd} to {closedPeriod.PeriodEnd:yyyy-MM-dd} is closed."),
            });
        }

        entry.Post();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CreateJournalEntryCommandHandler.MapToDto(entry);
    }
}
