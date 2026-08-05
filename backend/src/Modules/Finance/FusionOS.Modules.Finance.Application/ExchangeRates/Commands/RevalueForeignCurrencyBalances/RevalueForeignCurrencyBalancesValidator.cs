using FluentValidation;

namespace FusionOS.Modules.Finance.Application.ExchangeRates.Commands.RevalueForeignCurrencyBalances;

public sealed class RevalueForeignCurrencyBalancesValidator : AbstractValidator<RevalueForeignCurrencyBalancesCommand>
{
    public RevalueForeignCurrencyBalancesValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.CurrencyCode).NotEmpty().Length(3);
        RuleFor(x => x.BaseCurrencyCode).NotEmpty().Length(3);
        RuleFor(x => x.NewExchangeRate).GreaterThan(0);
        RuleFor(x => x.EffectiveDate).NotEmpty();
        RuleFor(x => x.GainLossAccountId).NotEmpty();
        RuleFor(x => x)
            .Must(x => !string.Equals(x.CurrencyCode.Trim(), x.BaseCurrencyCode.Trim(), StringComparison.OrdinalIgnoreCase))
            .WithMessage("Currency code and base currency code must differ.");
    }
}
