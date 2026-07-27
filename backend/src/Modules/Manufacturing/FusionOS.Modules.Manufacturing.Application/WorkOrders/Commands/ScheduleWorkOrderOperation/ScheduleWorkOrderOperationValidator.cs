using FluentValidation;

namespace FusionOS.Modules.Manufacturing.Application.WorkOrders.Commands.ScheduleWorkOrderOperation;

public sealed class ScheduleWorkOrderOperationValidator : AbstractValidator<ScheduleWorkOrderOperationCommand>
{
    public ScheduleWorkOrderOperationValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.WorkOrderId).NotEmpty();
        RuleFor(x => x.WorkOrderOperationId).NotEmpty();
        RuleFor(x => x.MachineId).NotEmpty();
        RuleFor(x => x.ScheduledEnd).GreaterThan(x => x.ScheduledStart)
            .WithMessage("Scheduled end must be after scheduled start.");
    }
}
