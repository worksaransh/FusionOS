using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.AccountingPeriods.Contracts;
using FusionOS.Modules.Finance.Application.JournalEntries.Commands.CreateJournalEntry;
using FusionOS.Modules.Finance.Application.JournalEntries.Contracts;
using FusionOS.SharedKernel.Context;
using MediatR;

namespace FusionOS.Modules.Finance.Application.JournalEntries.Commands.PostJournalEntry;

public sealed class PostJournalEntryCommandHandler : IRequestHandler<PostJournalEntryCommand, JournalEntryDto>
{
    private const string CreatePermission = "finance.journal-entry.create";
    private const string PostPermission = "finance.journal-entry.post";

    private readonly IJournalEntryRepository _repository;
    private readonly IAccountingPeriodRepository _accountingPeriodRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserContext _currentUser;
    private readonly ISegregationOfDutiesService _sodService;

    public PostJournalEntryCommandHandler(
        IJournalEntryRepository repository,
        IAccountingPeriodRepository accountingPeriodRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserContext currentUser,
        ISegregationOfDutiesService sodService)
    {
        _repository = repository;
        _accountingPeriodRepository = accountingPeriodRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _sodService = sodService;
    }

    public async Task<JournalEntryDto> Handle(PostJournalEntryCommand request, CancellationToken cancellationToken)
    {
        var entry = await _repository.GetByIdAsync(request.CompanyId, request.JournalEntryId, cancellationToken)
            ?? throw new KeyNotFoundException($"Journal entry '{request.JournalEntryId}' was not found.");

        // Segregation-of-Duties: if this company has an active SodConflictRule explicitly
        // naming finance.journal-entry.create + finance.journal-entry.post as conflicting, and
        // the posting user is the same user who created the entry, surface it as a SoD
        // violation (ValidationException). Mirrors ApprovePurchaseOrderCommandHandler.
        if (_currentUser.UserId is { } posterId && posterId == entry.CreatedBy)
        {
            if (await _sodService.HasConflictAsync(request.CompanyId, CreatePermission, PostPermission, cancellationToken))
            {
                throw new ValidationException(new[]
                {
                    new FluentValidation.Results.ValidationFailure(
                        nameof(request.JournalEntryId),
                        "Segregation of duties: the same user cannot both create and post this journal entry."),
                });
            }
        }

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
