using FluentValidation;

namespace FusionOS.Modules.Finance.Application.ExchangeRates.Commands.UpdateExchangeRate;

public sealed class UpdateExchangeRateValidator : AbstractValidator<UpdateExchangeRateCommand>
{
    public UpdateExchangeRateValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.ExchangeRateId).NotEmpty();
        RuleFor(x => x.Rate).GreaterThan(0);
        RuleFor(x => x.EffectiveDate).NotEmpty();
    }
}
