using FluentValidation;

namespace FusionOS.Modules.Manufacturing.Application.WorkOrders.Commands.CancelWorkOrder;

public sealed class CancelWorkOrderValidator : AbstractValidator<CancelWorkOrderCommand>
{
    public CancelWorkOrderValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.WorkOrderId).NotEmpty();
    }
}
