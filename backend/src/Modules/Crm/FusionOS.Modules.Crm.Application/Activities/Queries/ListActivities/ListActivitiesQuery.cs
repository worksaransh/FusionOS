using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Crm.Application.Activities.Contracts;

namespace FusionOS.Modules.Crm.Application.Activities.Queries.ListActivities;

/// <summary>EntityType/EntityId are optional — omit both for a company-wide feed, or supply both to see one record's history.</summary>
public sealed record ListActivitiesQuery(Guid CompanyId, string? EntityType = null, Guid? EntityId = null, int Page = 1, int PageSize = 25)
    : IQuery<PagedResult<ActivityDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "crm.activity.read" };
}
