using FluentValidation;

namespace FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Commands.CreateBillOfMaterials;

public sealed class CreateBillOfMaterialsValidator : AbstractValidator<CreateBillOfMaterialsCommand>
{
    public CreateBillOfMaterialsValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Lines).NotEmpty().WithMessage("A bill of materials must have at least one component line.");
        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.ComponentProductId).NotEmpty();
            line.RuleFor(l => l.Quantity).GreaterThan(0);
        });
    }
}
