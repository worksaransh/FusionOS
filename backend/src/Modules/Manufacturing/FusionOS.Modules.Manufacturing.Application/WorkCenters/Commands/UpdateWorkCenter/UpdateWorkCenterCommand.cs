using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Manufacturing.Application.WorkCenters.Contracts;

namespace FusionOS.Modules.Manufacturing.Application.WorkCenters.Commands.UpdateWorkCenter;

/// <summary>Update deliberately excludes Code — it's the immutable business key, same convention as UpdateBranchCommand/UpdateCostCenterCommand.</summary>
public sealed record UpdateWorkCenterCommand(Guid CompanyId, Guid WorkCenterId, string Name, decimal CapacityHoursPerDay)
    : ICommand<WorkCenterDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "manufacturing.work-center.update" };
    public string EntityType => nameof(Domain.WorkCenters.WorkCenter);
    public Guid EntityId => WorkCenterId;
    public string Action => "Updated";
}
