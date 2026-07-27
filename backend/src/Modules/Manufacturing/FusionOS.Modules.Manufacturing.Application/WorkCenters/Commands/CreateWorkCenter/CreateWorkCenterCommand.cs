using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Manufacturing.Application.WorkCenters.Contracts;

namespace FusionOS.Modules.Manufacturing.Application.WorkCenters.Commands.CreateWorkCenter;

public sealed record CreateWorkCenterCommand(
    Guid CompanyId,
    string Code,
    string Name,
    decimal CapacityHoursPerDay)
    : ICommand<WorkCenterDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "manufacturing.work-center.create" };
    public string EntityType => nameof(Domain.WorkCenters.WorkCenter);
    public Guid EntityId { get; init; }
    public string Action => "Created";
}
