namespace FusionOS.Modules.Quality.Application.Inspections.Contracts;

public sealed record InspectionItemDto(Guid Id, string Characteristic, bool? Passed, string? Notes);

public sealed record InspectionDto(
    Guid Id,
    string Type,
    Guid ReferenceId,
    string Status,
    IReadOnlyList<InspectionItemDto> Items);

/// <summary>Single place that turns an Inspection aggregate into its DTO, shared by every handler that returns one.</summary>
public static class InspectionMapper
{
    public static InspectionDto ToDto(Domain.Inspections.Inspection inspection) => new(
        inspection.Id,
        inspection.Type.ToString(),
        inspection.ReferenceId,
        inspection.Status.ToString(),
        inspection.Items.Select(i => new InspectionItemDto(i.Id, i.Characteristic, i.Passed, i.Notes)).ToList());
}
