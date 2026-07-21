using FluentValidation;

namespace FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Commands.ReorderRoutingOperations;

public sealed class ReorderRoutingOperationsValidator : AbstractValidator<ReorderRoutingOperationsCommand>
{
    public ReorderRoutingOperationsValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.BillOfMaterialsId).NotEmpty();
        RuleFor(x => x.OrderedOperationIds).NotEmpty().WithMessage("The reordered list of operation ids is required.");
        RuleForEach(x => x.OrderedOperationIds).NotEmpty();
    }
}
