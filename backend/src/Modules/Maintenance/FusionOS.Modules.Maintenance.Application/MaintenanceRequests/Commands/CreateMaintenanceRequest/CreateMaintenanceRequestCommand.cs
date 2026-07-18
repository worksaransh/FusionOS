using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Maintenance.Application.MaintenanceRequests.Contracts;
using FusionOS.Modules.Maintenance.Domain.MaintenanceRequests;

namespace FusionOS.Modules.Maintenance.Application.MaintenanceRequests.Commands.CreateMaintenanceRequest;

public sealed record CreateMaintenanceRequestCommand(Guid CompanyId, Guid AssetId, MaintenanceRequestType Type, string Description)
    : ICommand<MaintenanceRequestDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "maintenance.request.create" };
    public string EntityType => nameof(Domain.MaintenanceRequests.MaintenanceRequest);
    public Guid EntityId { get; init; }
    public string Action => "Created";
}
