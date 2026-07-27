using FusionOS.Modules.Crm.Application.Activities.Contracts;
using FusionOS.Modules.Crm.Application.Leads.Contracts;
using FusionOS.Modules.Crm.Application.Leads.Services;
using MediatR;

namespace FusionOS.Modules.Crm.Application.Leads.Commands.RecalculateLeadScore;

public sealed class RecalculateLeadScoreCommandHandler : IRequestHandler<RecalculateLeadScoreCommand, LeadDto>
{
    private readonly ILeadRepository _leadRepository;
    private readonly IActivityRepository _activityRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RecalculateLeadScoreCommandHandler(ILeadRepository leadRepository, IActivityRepository activityRepository, IUnitOfWork unitOfWork)
    {
        _leadRepository = leadRepository;
        _activityRepository = activityRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<LeadDto> Handle(RecalculateLeadScoreCommand request, CancellationToken cancellationToken)
    {
        var lead = await _leadRepository.GetByIdAsync(request.CompanyId, request.LeadId, cancellationToken)
            ?? throw new KeyNotFoundException($"Lead '{request.LeadId}' was not found.");

        // "At least one Activity logged" — Activity uses the same opaque (EntityType,
        // EntityId) convention as this command targets, so nameof(Domain.Leads.Lead) is
        // the same string CreateActivityCommand would have been given when logging one
        // against this lead.
        var activityCount = await _activityRepository.CountAsync(
            request.CompanyId, nameof(Domain.Leads.Lead), request.LeadId, cancellationToken);

        var score = LeadScoringService.Calculate(lead.Source, lead.AccountId, hasLoggedActivity: activityCount > 0);
        lead.UpdateScore(score);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return LeadMapper.ToDto(lead);
    }
}
