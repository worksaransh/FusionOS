using FluentValidation;

namespace FusionOS.Modules.Ai.Application.Forecasting.Queries.GetDemandForecast;

public sealed class GetDemandForecastValidator : AbstractValidator<GetDemandForecastQuery>
{
    public GetDemandForecastValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.HistoryDays).GreaterThan(0).LessThanOrEqualTo(3650);
        RuleFor(x => x.ForecastDays).GreaterThan(0).LessThanOrEqualTo(365);
    }
}
