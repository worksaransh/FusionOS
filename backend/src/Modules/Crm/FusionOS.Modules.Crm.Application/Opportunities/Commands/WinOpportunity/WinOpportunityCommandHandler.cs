using FusionOS.Modules.Crm.Application.Leads.Contracts;
using FusionOS.Modules.Crm.Application.Opportunities.Contracts;
using MediatR;

namespace FusionOS.Modules.Crm.Application.Opportunities.Commands.WinOpportunity;

public sealed class WinOpportunityCommandHandler : IRequestHandler<WinOpportunityCommand, OpportunityDto>
{
    private readonly IOpportunityRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public WinOpportunityCommandHandler(IOpportunityRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<OpportunityDto> Handle(WinOpportunityCommand request, CancellationToken cancellationToken)
    {
        var opportunity = await _repository.GetByIdAsync(request.CompanyId, request.OpportunityId, cancellationToken)
            ?? throw new KeyNotFoundException($"Opportunity '{request.OpportunityId}' was not found.");

        // Raises OpportunityWon; the outbox relays it to Kafka and Sales'
        // OpportunityWonConsumer creates the real Customer.
        opportunity.Win(request.CustomerCode);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return OpportunityMapper.ToDto(opportunity);
    }
}
