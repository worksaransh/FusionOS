using FluentValidation;

namespace FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Queries.GenerateMrpRequirements;

public sealed class GenerateMrpRequirementsValidator : AbstractValidator<GenerateMrpRequirementsQuery>
{
    public GenerateMrpRequirementsValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.DemandLines).NotEmpty().WithMessage("At least one demand line is required to run MRP.");
        RuleForEach(x => x.DemandLines).ChildRules(line =>
        {
            line.RuleFor(l => l.ProductId).NotEmpty();
            line.RuleFor(l => l.Quantity).GreaterThan(0);
        });
    }
}
