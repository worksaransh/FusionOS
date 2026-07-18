using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Crm.Application.Leads.Contracts;
using FusionOS.Modules.Crm.Application.Opportunities.Contracts;
using MediatR;

namespace FusionOS.Modules.Crm.Application.Opportunities.Commands.CreateOpportunity;

public sealed class CreateOpportunityCommandHandler : IRequestHandler<CreateOpportunityCommand, OpportunityDto>
{
    private readonly IOpportunityRepository _opportunityRepository;
    private readonly ILeadRepository _leadRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateOpportunityCommandHandler(
        IOpportunityRepository opportunityRepository,
        ILeadRepository leadRepository,
        IUnitOfWork unitOfWork)
    {
        _opportunityRepository = opportunityRepository;
        _leadRepository = leadRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<OpportunityDto> Handle(CreateOpportunityCommand request, CancellationToken cancellationToken)
    {
        var lead = await _leadRepository.GetByIdAsync(request.CompanyId, request.LeadId, cancellationToken)
            ?? throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.LeadId), $"Lead '{request.LeadId}' does not exist for this company."),
            });

        // Snapshot the prospect's name/email from the lead so winning can create the Sales
        // Customer without a cross-module read.
        var opportunity = Domain.Opportunities.Opportunity.Create(
            request.CompanyId, lead.Id, request.Name, lead.Name, lead.ContactEmail, request.EstimatedValue);

        // Same-module, same DbContext: mark the lead converted and add the opportunity, one SaveChanges.
        // Lead.MarkConverted enforces the lead is Qualified first (throws otherwise -> 409).
        lead.MarkConverted();
        await _opportunityRepository.AddAsync(opportunity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return OpportunityMapper.ToDto(opportunity);
    }
}
