using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.JournalEntries.Commands.CreateJournalEntry;
using FusionOS.Modules.Finance.Application.JournalEntries.Contracts;
using MediatR;

namespace FusionOS.Modules.Finance.Application.JournalEntries.Commands.PostJournalEntry;

public sealed class PostJournalEntryCommandHandler : IRequestHandler<PostJournalEntryCommand, JournalEntryDto>
{
    private readonly IJournalEntryRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public PostJournalEntryCommandHandler(IJournalEntryRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<JournalEntryDto> Handle(PostJournalEntryCommand request, CancellationToken cancellationToken)
    {
        var entry = await _repository.GetByIdAsync(request.CompanyId, request.JournalEntryId, cancellationToken)
            ?? throw new KeyNotFoundException($"Journal entry '{request.JournalEntryId}' was not found.");

        entry.Post();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CreateJournalEntryCommandHandler.MapToDto(entry);
    }
}
