using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Crm.Application.Accounts.Contracts;
using FusionOS.Modules.Crm.Application.Contacts.Contracts;
using FusionOS.Modules.Crm.Application.Leads.Contracts;
using MediatR;

namespace FusionOS.Modules.Crm.Application.Contacts.Commands.CreateContact;

public sealed class CreateContactCommandHandler : IRequestHandler<CreateContactCommand, ContactDto>
{
    private readonly IContactRepository _contactRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly ILeadRepository _leadRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateContactCommandHandler(
        IContactRepository contactRepository,
        IAccountRepository accountRepository,
        ILeadRepository leadRepository,
        IUnitOfWork unitOfWork)
    {
        _contactRepository = contactRepository;
        _accountRepository = accountRepository;
        _leadRepository = leadRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ContactDto> Handle(CreateContactCommand request, CancellationToken cancellationToken)
    {
        if (request.AccountId is { } accountId && !await _accountRepository.ExistsAsync(request.CompanyId, accountId, cancellationToken))
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.AccountId), $"Account '{accountId}' does not exist for this company."),
            });
        }

        if (request.LeadId is { } leadId && await _leadRepository.GetByIdAsync(request.CompanyId, leadId, cancellationToken) is null)
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.LeadId), $"Lead '{leadId}' does not exist for this company."),
            });
        }

        // Duplicate check runs BEFORE creating, but is advisory only — see ContactDto.
        // IsPossibleDuplicate. Two different real contacts (e.g. two employees at the same
        // company sharing a general inbox) can legitimately share an email, so this warns
        // rather than blocks — unlike a genuine uniqueness constraint enforced elsewhere in
        // this codebase (e.g. Product.Sku), where a collision really is invalid.
        var normalizedEmail = string.IsNullOrWhiteSpace(request.Email)
            ? null
            : request.Email.Trim().ToLowerInvariant();
        var isPossibleDuplicate = normalizedEmail is not null
            && await _contactRepository.IsEmailAlreadyUsedAsync(request.CompanyId, normalizedEmail, cancellationToken);

        var contact = Domain.Contacts.Contact.Create(
            request.CompanyId, request.Name, request.Email, request.Phone, request.Title, request.AccountId, request.LeadId);

        await _contactRepository.AddAsync(contact, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ContactMapper.ToDto(contact, isPossibleDuplicate);
    }
}
