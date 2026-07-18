using FluentValidation;

namespace FusionOS.Modules.Crm.Application.Opportunities.Commands.CreateOpportunity;

public sealed class CreateOpportunityValidator : AbstractValidator<CreateOpportunityCommand>
{
    public CreateOpportunityValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.LeadId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.EstimatedValue).GreaterThanOrEqualTo(0);
    }
}
