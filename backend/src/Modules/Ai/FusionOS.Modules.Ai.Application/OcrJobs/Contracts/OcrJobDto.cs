namespace FusionOS.Modules.Ai.Application.OcrJobs.Contracts;

public sealed record OcrJobDto(
    Guid Id,
    Guid DocumentId,
    string Status,
    string? ExtractedFieldsJson,
    string? FailureReason,
    string? DraftReferenceType,
    Guid? DraftReferenceId,
    DateTimeOffset CreatedAt);

/// <summary>Single place that turns an OcrJob aggregate into its DTO, shared by every handler that returns one.</summary>
public static class OcrJobMapper
{
    public static OcrJobDto ToDto(Domain.OcrJobs.OcrJob job) => new(
        job.Id,
        job.DocumentId,
        job.Status.ToString(),
        job.ExtractedFieldsJson,
        job.FailureReason,
        job.DraftReferenceType,
        job.DraftReferenceId,
        job.CreatedAt);
}
