using FluentValidation;
using FusionOS.Modules.Crm.Domain.Opportunities;

namespace FusionOS.Modules.Crm.Application.Opportunities.Commands.ChangeOpportunityStage;

public sealed class ChangeOpportunityStageValidator : AbstractValidator<ChangeOpportunityStageCommand>
{
    private static readonly OpportunityStage[] AllowedStages =
    {
        OpportunityStage.Open,
        OpportunityStage.Prospecting,
        OpportunityStage.Qualification,
        OpportunityStage.Proposal,
        OpportunityStage.Negotiation,
    };

    public ChangeOpportunityStageValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.OpportunityId).NotEmpty();
        RuleFor(x => x.Stage)
            .Must(stage => AllowedStages.Contains(stage))
            .WithMessage("Stage must be one of Open, Prospecting, Qualification, Proposal, Negotiation — use the win/lose actions to close an opportunity.");
    }
}
