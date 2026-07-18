using FluentValidation;

namespace FusionOS.Modules.Finance.Application.TaxRates.Commands.CreateTaxRate;

public sealed class CreateTaxRateValidator : AbstractValidator<CreateTaxRateCommand>
{
    public CreateTaxRateValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.TaxJurisdictionId).NotEmpty();
        RuleFor(x => x.Code).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Percentage).InclusiveBetween(0, 100);
    }
}
