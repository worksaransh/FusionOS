using FluentValidation;

namespace FusionOS.Modules.Maintenance.Application.MaintenanceSchedules.Commands.CreateMaintenanceSchedule;

public sealed class CreateMaintenanceScheduleValidator : AbstractValidator<CreateMaintenanceScheduleCommand>
{
    public CreateMaintenanceScheduleValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.AssetId).NotEmpty();
        RuleFor(x => x.Description).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.Frequency).IsInEnum();
    }
}
