using FluentValidation;

namespace FusionOS.Modules.Finance.Application.TaxJurisdictions.Commands.CreateTaxJurisdiction;

public sealed class CreateTaxJurisdictionValidator : AbstractValidator<CreateTaxJurisdictionCommand>
{
    public CreateTaxJurisdictionValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.Code).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}
