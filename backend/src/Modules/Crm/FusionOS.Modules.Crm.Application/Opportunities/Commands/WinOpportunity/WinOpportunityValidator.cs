using FluentValidation;

namespace FusionOS.Modules.Crm.Application.Opportunities.Commands.WinOpportunity;

public sealed class WinOpportunityValidator : AbstractValidator<WinOpportunityCommand>
{
    public WinOpportunityValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.OpportunityId).NotEmpty();
        RuleFor(x => x.CustomerCode).NotEmpty().MaximumLength(50);
    }
}
