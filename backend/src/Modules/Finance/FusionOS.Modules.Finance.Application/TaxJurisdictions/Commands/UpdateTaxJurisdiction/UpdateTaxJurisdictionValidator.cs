using FluentValidation;

namespace FusionOS.Modules.Finance.Application.TaxJurisdictions.Commands.UpdateTaxJurisdiction;

public sealed class UpdateTaxJurisdictionValidator : AbstractValidator<UpdateTaxJurisdictionCommand>
{
    public UpdateTaxJurisdictionValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.TaxJurisdictionId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}
