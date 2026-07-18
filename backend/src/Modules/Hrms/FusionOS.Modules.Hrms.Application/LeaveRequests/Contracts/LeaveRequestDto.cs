namespace FusionOS.Modules.Hrms.Application.LeaveRequests.Contracts;

public sealed record LeaveRequestDto(
    Guid Id,
    Guid EmployeeId,
    string Type,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    string? Reason,
    string Status);

/// <summary>Single place that turns a LeaveRequest aggregate into its DTO, shared by every handler that returns one.</summary>
public static class LeaveRequestMapper
{
    public static LeaveRequestDto ToDto(Domain.LeaveRequests.LeaveRequest request) => new(
        request.Id,
        request.EmployeeId,
        request.Type.ToString(),
        request.StartDate,
        request.EndDate,
        request.Reason,
        request.Status.ToString());
}
