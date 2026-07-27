using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Crm.Application.Dashboard.Contracts;

namespace FusionOS.Modules.Crm.Application.Dashboard.Queries.GetCrmDashboard;

public sealed record GetCrmDashboardQuery(Guid CompanyId) : IQuery<CrmDashboardDto>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "crm.dashboard.read" };
}
