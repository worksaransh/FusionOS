using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Manufacturing.Application.WorkCenters.Contracts;

namespace FusionOS.Modules.Manufacturing.Application.WorkCenters.Commands.DeactivateWorkCenter;

public sealed record DeactivateWorkCenterCommand(Guid CompanyId, Guid WorkCenterId)
    : ICommand<WorkCenterDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "manufacturing.work-center.deactivate" };
    public string EntityType => nameof(Domain.WorkCenters.WorkCenter);
    public Guid EntityId => WorkCenterId;
    public string Action => "Deactivated";
}
