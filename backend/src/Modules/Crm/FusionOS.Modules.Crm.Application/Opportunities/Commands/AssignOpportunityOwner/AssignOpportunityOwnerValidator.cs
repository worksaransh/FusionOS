using FluentValidation;

namespace FusionOS.Modules.Crm.Application.Opportunities.Commands.AssignOpportunityOwner;

public sealed class AssignOpportunityOwnerValidator : AbstractValidator<AssignOpportunityOwnerCommand>
{
    public AssignOpportunityOwnerValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.OpportunityId).NotEmpty();
    }
}
