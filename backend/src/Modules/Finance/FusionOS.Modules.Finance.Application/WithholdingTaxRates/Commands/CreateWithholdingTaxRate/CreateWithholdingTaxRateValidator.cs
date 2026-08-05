using FluentValidation;

namespace FusionOS.Modules.Finance.Application.WithholdingTaxRates.Commands.CreateWithholdingTaxRate;

public sealed class CreateWithholdingTaxRateValidator : AbstractValidator<CreateWithholdingTaxRateCommand>
{
    public CreateWithholdingTaxRateValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.SectionCode).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Percentage).InclusiveBetween(0, 100);
    }
}
