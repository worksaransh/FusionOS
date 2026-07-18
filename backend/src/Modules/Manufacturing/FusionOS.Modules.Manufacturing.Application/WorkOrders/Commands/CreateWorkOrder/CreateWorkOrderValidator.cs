using FluentValidation;

namespace FusionOS.Modules.Manufacturing.Application.WorkOrders.Commands.CreateWorkOrder;

public sealed class CreateWorkOrderValidator : AbstractValidator<CreateWorkOrderCommand>
{
    public CreateWorkOrderValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.BillOfMaterialsId).NotEmpty();
        RuleFor(x => x.WarehouseId).NotEmpty();
        RuleFor(x => x.QuantityToProduce).GreaterThan(0);
    }
}
