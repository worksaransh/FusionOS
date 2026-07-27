namespace FusionOS.Modules.IntegrationHub.Application.Migrations.Contracts;

public sealed record MigrationStagingRecordDto(
    Guid Id,
    Guid MigrationJobId,
    string RecordType,
    string RawDataJson,
    string MappingStatus,
    string? ValidationErrors,
    string? TargetEntityType,
    Guid? TargetEntityId,
    DateTimeOffset CreatedAt);

/// <summary>Single place that turns a MigrationStagingRecord aggregate into its DTO, shared by every handler that returns one.</summary>
public static class MigrationStagingRecordMapper
{
    public static MigrationStagingRecordDto ToDto(Domain.Migrations.MigrationStagingRecord record) => new(
        record.Id, record.MigrationJobId, record.RecordType, record.RawDataJson,
        record.MappingStatus.ToString(), record.ValidationErrors, record.TargetEntityType,
        record.TargetEntityId, record.CreatedAt);
}
