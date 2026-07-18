namespace FusionOS.Modules.BusinessIntelligence.Application.KpiSnapshots.Contracts;

public sealed record KpiSnapshotDto(Guid Id, Guid KpiDefinitionId, decimal Value, DateTimeOffset RecordedAt, string? Notes);

/// <summary>Single place that turns a KpiSnapshot aggregate into its DTO, shared by every handler that returns one.</summary>
public static class KpiSnapshotMapper
{
    public static KpiSnapshotDto ToDto(Domain.KpiSnapshots.KpiSnapshot snapshot) =>
        new(snapshot.Id, snapshot.KpiDefinitionId, snapshot.Value, snapshot.RecordedAt, snapshot.Notes);
}
