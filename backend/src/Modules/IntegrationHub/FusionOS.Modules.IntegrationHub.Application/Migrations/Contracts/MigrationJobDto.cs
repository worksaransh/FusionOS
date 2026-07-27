namespace FusionOS.Modules.IntegrationHub.Application.Migrations.Contracts;

public sealed record MigrationJobDto(
    Guid Id,
    string SourceSystem,
    string Status,
    int TotalRecordsExtracted,
    int TotalRecordsLoaded,
    int TotalRecordsFailed,
    string? ReconciliationNote,
    DateTimeOffset CreatedAt);

/// <summary>Single place that turns a MigrationJob aggregate into its DTO, shared by every handler that returns one.</summary>
public static class MigrationJobMapper
{
    public static MigrationJobDto ToDto(Domain.Migrations.MigrationJob job) => new(
        job.Id, job.SourceSystem, job.Status.ToString(), job.TotalRecordsExtracted,
        job.TotalRecordsLoaded, job.TotalRecordsFailed, job.ReconciliationNote, job.CreatedAt);
}
