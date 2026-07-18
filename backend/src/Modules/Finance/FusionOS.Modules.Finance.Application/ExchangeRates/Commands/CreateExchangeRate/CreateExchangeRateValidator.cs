using FluentValidation;

namespace FusionOS.Modules.Finance.Application.ExchangeRates.Commands.CreateExchangeRate;

public sealed class CreateExchangeRateValidator : AbstractValidator<CreateExchangeRateCommand>
{
    public CreateExchangeRateValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.FromCurrencyCode).NotEmpty().Length(3);
        RuleFor(x => x.ToCurrencyCode).NotEmpty().Length(3);
        RuleFor(x => x.Rate).GreaterThan(0);
        RuleFor(x => x.EffectiveDate).NotEmpty();
    }
}
