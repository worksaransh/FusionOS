using FluentValidation;

namespace FusionOS.Modules.Crm.Application.Leads.Commands.RecalculateLeadScore;

public sealed class RecalculateLeadScoreValidator : AbstractValidator<RecalculateLeadScoreCommand>
{
    public RecalculateLeadScoreValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.LeadId).NotEmpty();
    }
}
