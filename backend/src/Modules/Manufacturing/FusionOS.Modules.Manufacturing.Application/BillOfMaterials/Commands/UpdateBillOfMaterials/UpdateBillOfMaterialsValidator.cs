using FluentValidation;

namespace FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Commands.UpdateBillOfMaterials;

public sealed class UpdateBillOfMaterialsValidator : AbstractValidator<UpdateBillOfMaterialsCommand>
{
    public UpdateBillOfMaterialsValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.BillOfMaterialsId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ProductId).NotEmpty();
    }
}
