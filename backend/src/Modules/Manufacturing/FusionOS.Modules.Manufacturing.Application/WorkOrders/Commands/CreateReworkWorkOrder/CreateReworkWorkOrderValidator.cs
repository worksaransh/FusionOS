using FluentValidation;

namespace FusionOS.Modules.Manufacturing.Application.WorkOrders.Commands.CreateReworkWorkOrder;

public sealed class CreateReworkWorkOrderValidator : AbstractValidator<CreateReworkWorkOrderCommand>
{
    public CreateReworkWorkOrderValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.SourceWorkOrderId).NotEmpty();
        RuleFor(x => x.QuantityToRework).GreaterThan(0);
    }
}
