using FluentValidation;

namespace FusionOS.Modules.Manufacturing.Application.WorkOrders.Commands.ReturnMaterialFromWorkOrder;

public sealed class ReturnMaterialFromWorkOrderValidator : AbstractValidator<ReturnMaterialFromWorkOrderCommand>
{
    public ReturnMaterialFromWorkOrderValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.WorkOrderId).NotEmpty();
        RuleFor(x => x.ComponentProductId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
    }
}
