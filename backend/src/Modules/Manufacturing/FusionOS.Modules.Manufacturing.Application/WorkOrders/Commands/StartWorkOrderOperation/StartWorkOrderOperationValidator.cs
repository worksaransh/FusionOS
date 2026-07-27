using FluentValidation;

namespace FusionOS.Modules.Manufacturing.Application.WorkOrders.Commands.StartWorkOrderOperation;

public sealed class StartWorkOrderOperationValidator : AbstractValidator<StartWorkOrderOperationCommand>
{
    public StartWorkOrderOperationValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.WorkOrderId).NotEmpty();
        RuleFor(x => x.WorkOrderOperationId).NotEmpty();
    }
}
