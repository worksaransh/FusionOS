using FluentValidation;

namespace FusionOS.Modules.Manufacturing.Application.WorkOrders.Commands.IssueMaterialToWorkOrder;

public sealed class IssueMaterialToWorkOrderValidator : AbstractValidator<IssueMaterialToWorkOrderCommand>
{
    public IssueMaterialToWorkOrderValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.WorkOrderId).NotEmpty();
        RuleFor(x => x.ComponentProductId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
    }
}
