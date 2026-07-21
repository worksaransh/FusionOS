using FluentValidation;

namespace FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Commands.AddRoutingOperation;

public sealed class AddRoutingOperationValidator : AbstractValidator<AddRoutingOperationCommand>
{
    public AddRoutingOperationValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.BillOfMaterialsId).NotEmpty();
        RuleFor(x => x.OperationName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.WorkCenter).NotEmpty().MaximumLength(100);
        RuleFor(x => x.StandardMinutes).GreaterThanOrEqualTo(0);
    }
}
