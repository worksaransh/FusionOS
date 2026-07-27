using FluentValidation;

namespace FusionOS.Modules.Crm.Application.Opportunities.Commands.UpdateOpportunityForecast;

public sealed class UpdateOpportunityForecastValidator : AbstractValidator<UpdateOpportunityForecastCommand>
{
    public UpdateOpportunityForecastValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.OpportunityId).NotEmpty();
        RuleFor(x => x.Probability).InclusiveBetween(0, 100).When(x => x.Probability is not null);
    }
}
