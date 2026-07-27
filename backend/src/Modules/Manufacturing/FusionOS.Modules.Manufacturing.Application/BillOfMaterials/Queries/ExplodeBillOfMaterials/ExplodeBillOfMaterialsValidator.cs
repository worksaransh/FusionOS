using FluentValidation;

namespace FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Queries.ExplodeBillOfMaterials;

public sealed class ExplodeBillOfMaterialsValidator : AbstractValidator<ExplodeBillOfMaterialsQuery>
{
    public ExplodeBillOfMaterialsValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.BillOfMaterialsId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
    }
}
