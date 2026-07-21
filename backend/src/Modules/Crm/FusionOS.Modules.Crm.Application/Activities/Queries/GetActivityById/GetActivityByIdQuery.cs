using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Crm.Application.Activities.Contracts;

namespace FusionOS.Modules.Crm.Application.Activities.Queries.GetActivityById;

public sealed record GetActivityByIdQuery(Guid CompanyId, Guid ActivityId) : IQuery<ActivityDto>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "crm.activity.read" };
}
