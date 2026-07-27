namespace FusionOS.Modules.Manufacturing.Application.WorkCenters.Contracts;

/// <summary>Single place that turns a WorkCenter aggregate into its DTO, shared by every handler that returns one.</summary>
public static class WorkCenterMapper
{
    public static WorkCenterDto ToDto(Domain.WorkCenters.WorkCenter workCenter) => new(
        workCenter.Id,
        workCenter.Code,
        workCenter.Name,
        workCenter.CapacityHoursPerDay,
        workCenter.IsActive);
}
