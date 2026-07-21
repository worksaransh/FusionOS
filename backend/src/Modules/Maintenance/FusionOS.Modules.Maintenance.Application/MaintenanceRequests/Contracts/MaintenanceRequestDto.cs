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
    int? ActualDowntimeMinutes);

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
        request.ActualDowntimeMinutes);
}
