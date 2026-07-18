using FluentValidation;

namespace FusionOS.Modules.Finance.Application.TaxRates.Commands.UpdateTaxRate;

public sealed class UpdateTaxRateValidator : AbstractValidator<UpdateTaxRateCommand>
{
    public UpdateTaxRateValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.TaxRateId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Percentage).InclusiveBetween(0, 100);
    }
}
