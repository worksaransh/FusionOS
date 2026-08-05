using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Quality.Application.InspectionPlans.Contracts;
using FusionOS.Modules.Quality.Domain.Inspections;

namespace FusionOS.Modules.Quality.Application.InspectionPlans.Commands.UpdateInspectionPlan;

/// <summary>Update replaces the whole characteristic list, same convention as UpdateBranchCommand/UpdateCostCenterCommand/UpdateCompanyCommand.</summary>
public sealed record UpdateInspectionPlanCommand(Guid CompanyId, Guid InspectionPlanId, string Name, InspectionType InspectionType, IReadOnlyList<string> CharacteristicNames)
    : ICommand<InspectionPlanDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "quality.inspection-plan.update" };
    public string EntityType => nameof(Domain.InspectionPlans.InspectionPlan);
    public Guid EntityId => InspectionPlanId;
    public string Action => "Updated";
}
