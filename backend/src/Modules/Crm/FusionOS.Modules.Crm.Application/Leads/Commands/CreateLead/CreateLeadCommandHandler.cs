using FusionOS.Modules.Crm.Application.Leads.Contracts;
using FusionOS.Modules.Crm.Application.Leads.Services;
using MediatR;

namespace FusionOS.Modules.Crm.Application.Leads.Commands.CreateLead;

public sealed class CreateLeadCommandHandler : IRequestHandler<CreateLeadCommand, LeadDto>
{
    private readonly ILeadRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateLeadCommandHandler(ILeadRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<LeadDto> Handle(CreateLeadCommand request, CancellationToken cancellationToken)
    {
        // Duplicate check runs BEFORE creating, but is advisory only — see LeadDto.
        // IsPossibleDuplicate. Two different real people can legitimately share a contact
        // email (e.g. a shared team inbox, or two leads independently submitted the same
        // webform), so this warns the caller rather than blocking creation — unlike a
        // genuine uniqueness constraint (e.g. Product.Sku) enforced elsewhere in this
        // codebase, where a collision really is invalid.
        var normalizedEmail = string.IsNullOrWhiteSpace(request.ContactEmail)
            ? null
            : request.ContactEmail.Trim().ToLowerInvariant();
        var isPossibleDuplicate = normalizedEmail is not null
            && await _repository.IsEmailAlreadyUsedAsync(request.CompanyId, normalizedEmail, cancellationToken);

        var lead = Domain.Leads.Lead.Create(request.CompanyId, request.Name, request.ContactEmail, request.ContactPhone, request.Source);

        // Initial score at creation time: a brand-new Lead never has an AccountId (only
        // AssignAccount sets that, later) and can't yet have a logged Activity against an
        // id that didn't exist a moment ago — so this only ever reflects the
        // Source-attribution signal. RecalculateLeadScoreCommand is how the fuller picture
        // gets computed once the lead has gained an Account and/or logged Activities.
        lead.UpdateScore(LeadScoringService.Calculate(lead.Source, lead.AccountId, hasLoggedActivity: false));

        await _repository.AddAsync(lead, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return LeadMapper.ToDto(lead, isPossibleDuplicate);
    }
}
