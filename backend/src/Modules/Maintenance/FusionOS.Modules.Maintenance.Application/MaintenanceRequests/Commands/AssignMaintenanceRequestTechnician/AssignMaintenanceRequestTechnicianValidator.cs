using FluentValidation;

namespace FusionOS.Modules.Maintenance.Application.MaintenanceRequests.Commands.AssignMaintenanceRequestTechnician;

public sealed class AssignMaintenanceRequestTechnicianValidator : AbstractValidator<AssignMaintenanceRequestTechnicianCommand>
{
    public AssignMaintenanceRequestTechnicianValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.MaintenanceRequestId).NotEmpty();
        RuleFor(x => x.TechnicianUserId).NotEmpty();
    }
}
