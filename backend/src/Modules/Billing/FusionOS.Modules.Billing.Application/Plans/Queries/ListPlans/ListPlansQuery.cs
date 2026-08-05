using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Billing.Application.Plans.Contracts;

namespace FusionOS.Modules.Billing.Application.Plans.Queries.ListPlans;

/// <summary>No CompanyId — Plan is global catalog data, not scoped to a tenant (see Plan's class doc comment).</summary>
public sealed record ListPlansQuery(int Page = 1, int PageSize = 25) : IQuery<PagedResult<PlanDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "billing.plan.read" };
}
