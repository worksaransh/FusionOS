using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Quality.Application.InspectionPlans.Contracts;
using FusionOS.Modules.Quality.Domain.Inspections;

namespace FusionOS.Modules.Quality.Application.InspectionPlans.Queries.ListInspectionPlans;

/// <summary>InspectionType is optional — omitted, this lists every plan for the company; supplied, it scopes to plans defined for that inspection type.</summary>
public sealed record ListInspectionPlansQuery(Guid CompanyId, InspectionType? InspectionType = null, int Page = 1, int PageSize = 25)
    : IQuery<PagedResult<InspectionPlanDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "quality.inspection-plan.read" };
}
