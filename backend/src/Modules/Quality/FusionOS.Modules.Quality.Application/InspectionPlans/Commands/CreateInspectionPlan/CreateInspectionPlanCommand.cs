using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Quality.Application.InspectionPlans.Contracts;
using FusionOS.Modules.Quality.Domain.Inspections;

namespace FusionOS.Modules.Quality.Application.InspectionPlans.Commands.CreateInspectionPlan;

public sealed record CreateInspectionPlanCommand(Guid CompanyId, string Name, InspectionType InspectionType, IReadOnlyList<string> CharacteristicNames)
    : ICommand<InspectionPlanDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "quality.inspection-plan.create" };
    public string EntityType => nameof(Domain.InspectionPlans.InspectionPlan);
    public Guid EntityId { get; init; }
    public string Action => "Created";
}
