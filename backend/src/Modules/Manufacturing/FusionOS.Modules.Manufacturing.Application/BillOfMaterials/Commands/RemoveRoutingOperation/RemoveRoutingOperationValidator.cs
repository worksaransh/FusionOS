using FluentValidation;

namespace FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Commands.RemoveRoutingOperation;

public sealed class RemoveRoutingOperationValidator : AbstractValidator<RemoveRoutingOperationCommand>
{
    public RemoveRoutingOperationValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.BillOfMaterialsId).NotEmpty();
        RuleFor(x => x.OperationId).NotEmpty();
    }
}
