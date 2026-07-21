using FluentValidation;

namespace FusionOS.Modules.Manufacturing.Application.WorkOrders.Commands.CompleteWorkOrder;

public sealed class CompleteWorkOrderValidator : AbstractValidator<CompleteWorkOrderCommand>
{
    public CompleteWorkOrderValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.WorkOrderId).NotEmpty();
        RuleFor(x => x.QuantityGoodProduced).GreaterThanOrEqualTo(0).When(x => x.QuantityGoodProduced.HasValue);
        RuleFor(x => x.QuantityScrapped).GreaterThanOrEqualTo(0).When(x => x.QuantityScrapped.HasValue);
    }
}
