namespace FusionOS.Modules.Maintenance.Application.MaintenanceRequests.Contracts;

public sealed record MaintenanceRequestDto(
    Guid Id,
    Guid AssetId,
    string Type,
    string Description,
    string Status,
    DateTimeOffset ReportedAt,
    DateTimeOffset? CompletedAt,
    string? ResolutionNotes,
    Guid? AssignedTechnicianUserId,
    int? ActualDowntimeMinutes,
    decimal? LaborHours,
    decimal? LaborRatePerHour,
    decimal TotalCost,
    IReadOnlyList<SparePartUsageDto> SparePartUsages,
    IReadOnlyList<MaintenanceRequestChecklistItemDto> ChecklistItems);

/// <summary>One spare part consumed against a maintenance request — see SparePartUsage's own doc comment for the ownership/opaque-reference conventions it follows.</summary>
public sealed record SparePartUsageDto(Guid Id, Guid ProductId, Guid WarehouseId, decimal QuantityUsed, decimal UnitCost, DateTimeOffset UsedAt);

/// <summary>One checklist step snapshotted onto a maintenance request — see MaintenanceRequestChecklistItem's own doc comment for the snapshot-at-creation convention it follows.</summary>
public sealed record MaintenanceRequestChecklistItemDto(Guid Id, string Description, bool IsCompleted, DateTimeOffset? CompletedAt);

/// <summary>Single place that turns a MaintenanceRequest aggregate into its DTO, shared by every handler that returns one.</summary>
public static class MaintenanceRequestMapper
{
    public static MaintenanceRequestDto ToDto(Domain.MaintenanceRequests.MaintenanceRequest request) => new(
        request.Id,
        request.AssetId,
        request.Type.ToString(),
        request.Description,
        request.Status.ToString(),
        request.ReportedAt,
        request.CompletedAt,
        request.ResolutionNotes,
        request.AssignedTechnicianUserId,
        request.ActualDowntimeMinutes,
        request.LaborHours,
        request.LaborRatePerHour,
        request.TotalCost,
        request.SparePartUsages.Select(p => new SparePartUsageDto(p.Id, p.ProductId, p.WarehouseId, p.QuantityUsed, p.UnitCost, p.UsedAt)).ToList(),
        request.ChecklistItems.Select(i => new MaintenanceRequestChecklistItemDto(i.Id, i.Description, i.IsCompleted, i.CompletedAt)).ToList());
}
