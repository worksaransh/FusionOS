using FluentValidation;

namespace FusionOS.Modules.Manufacturing.Application.WorkOrders.Commands.CompleteWorkOrderOperation;

public sealed class CompleteWorkOrderOperationValidator : AbstractValidator<CompleteWorkOrderOperationCommand>
{
    public CompleteWorkOrderOperationValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.WorkOrderId).NotEmpty();
        RuleFor(x => x.WorkOrderOperationId).NotEmpty();
    }
}
