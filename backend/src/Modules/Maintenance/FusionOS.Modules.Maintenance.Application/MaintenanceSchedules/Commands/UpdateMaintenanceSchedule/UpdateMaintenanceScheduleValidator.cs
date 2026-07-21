using FluentValidation;

namespace FusionOS.Modules.Maintenance.Application.MaintenanceSchedules.Commands.UpdateMaintenanceSchedule;

public sealed class UpdateMaintenanceScheduleValidator : AbstractValidator<UpdateMaintenanceScheduleCommand>
{
    public UpdateMaintenanceScheduleValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.MaintenanceScheduleId).NotEmpty();
        RuleFor(x => x.Description).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.Frequency).IsInEnum();
    }
}
