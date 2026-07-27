namespace FusionOS.Modules.IntegrationHub.Application.ConnectorSyncRuns.Contracts;

public sealed record ConnectorSyncRunDto(
    Guid Id,
    Guid ConnectorConnectionId,
    DateTimeOffset StartedAt,
    DateTimeOffset? CompletedAt,
    string Status,
    int RecordsProcessed,
    string? ErrorMessage);

/// <summary>Single place that turns a ConnectorSyncRun aggregate into its DTO, shared by every handler that returns one.</summary>
public static class ConnectorSyncRunMapper
{
    public static ConnectorSyncRunDto ToDto(Domain.ConnectorSyncRuns.ConnectorSyncRun run) =>
        new(run.Id, run.ConnectorConnectionId, run.StartedAt, run.CompletedAt, run.Status.ToString(), run.RecordsProcessed, run.ErrorMessage);
}
