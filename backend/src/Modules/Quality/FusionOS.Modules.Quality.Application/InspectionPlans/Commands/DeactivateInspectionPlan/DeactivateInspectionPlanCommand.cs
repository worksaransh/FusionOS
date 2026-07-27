using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Quality.Application.InspectionPlans.Contracts;

namespace FusionOS.Modules.Quality.Application.InspectionPlans.Commands.DeactivateInspectionPlan;

public sealed record DeactivateInspectionPlanCommand(Guid CompanyId, Guid InspectionPlanId)
    : ICommand<InspectionPlanDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "quality.inspection-plan.deactivate" };
    public string EntityType => nameof(Domain.InspectionPlans.InspectionPlan);
    public Guid EntityId => InspectionPlanId;
    public string Action => "Deactivated";
}
