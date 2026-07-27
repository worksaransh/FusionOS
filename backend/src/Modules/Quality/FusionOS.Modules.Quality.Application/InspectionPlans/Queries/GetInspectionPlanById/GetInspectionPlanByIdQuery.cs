using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Quality.Application.InspectionPlans.Contracts;

namespace FusionOS.Modules.Quality.Application.InspectionPlans.Queries.GetInspectionPlanById;

public sealed record GetInspectionPlanByIdQuery(Guid CompanyId, Guid InspectionPlanId) : IQuery<InspectionPlanDto>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "quality.inspection-plan.read" };
}
