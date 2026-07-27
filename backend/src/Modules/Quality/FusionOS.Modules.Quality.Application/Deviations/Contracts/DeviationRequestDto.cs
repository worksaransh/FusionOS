namespace FusionOS.Modules.Quality.Application.Deviations.Contracts;

public sealed record DeviationRequestDto(
    Guid Id,
    Guid? InspectionId,
    string Reason,
    string Status,
    Guid RequestedByUserId,
    Guid? ApprovedOrRejectedByUserId,
    string? DecisionNote);

/// <summary>Single place that turns a DeviationRequest aggregate into its DTO, shared by every handler that returns one.</summary>
public static class DeviationRequestMapper
{
    public static DeviationRequestDto ToDto(Domain.Deviations.DeviationRequest deviationRequest) => new(
        deviationRequest.Id,
        deviationRequest.InspectionId,
        deviationRequest.Reason,
        deviationRequest.Status.ToString(),
        deviationRequest.RequestedByUserId,
        deviationRequest.ApprovedOrRejectedByUserId,
        deviationRequest.DecisionNote);
}
