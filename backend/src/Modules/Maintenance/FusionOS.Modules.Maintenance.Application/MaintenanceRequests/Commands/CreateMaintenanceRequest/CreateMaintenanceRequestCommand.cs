using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Maintenance.Application.MaintenanceRequests.Contracts;
using FusionOS.Modules.Maintenance.Domain.MaintenanceRequests;

namespace FusionOS.Modules.Maintenance.Application.MaintenanceRequests.Commands.CreateMaintenanceRequest;

/// <summary>ChecklistTemplateId is an optional convenience (2026-07-23 follow-up) — when supplied, the handler snapshots that ChecklistTemplates.ChecklistTemplate's step descriptions onto this request at creation time (see MaintenanceRequest's own doc comment). Omitted, a request is created with no checklist, same as before this follow-up.</summary>
public sealed record CreateMaintenanceRequestCommand(Guid CompanyId, Guid AssetId, MaintenanceRequestType Type, string Description, Guid? ChecklistTemplateId = null)
    : ICommand<MaintenanceRequestDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "maintenance.request.create" };
    public string EntityType => nameof(Domain.MaintenanceRequests.MaintenanceRequest);
    public Guid EntityId { get; init; }
    public string Action => "Created";
}
