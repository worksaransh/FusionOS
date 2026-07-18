using FusionOS.Modules.Crm.Application.Leads.Contracts;
using FusionOS.Modules.Crm.Application.Opportunities.Contracts;
using MediatR;

namespace FusionOS.Modules.Crm.Application.Opportunities.Commands.LoseOpportunity;

public sealed class LoseOpportunityCommandHandler : IRequestHandler<LoseOpportunityCommand, OpportunityDto>
{
    private readonly IOpportunityRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public LoseOpportunityCommandHandler(IOpportunityRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<OpportunityDto> Handle(LoseOpportunityCommand request, CancellationToken cancellationToken)
    {
        var opportunity = await _repository.GetByIdAsync(request.CompanyId, request.OpportunityId, cancellationToken)
            ?? throw new KeyNotFoundException($"Opportunity '{request.OpportunityId}' was not found.");

        opportunity.Lose();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return OpportunityMapper.ToDto(opportunity);
    }
}
