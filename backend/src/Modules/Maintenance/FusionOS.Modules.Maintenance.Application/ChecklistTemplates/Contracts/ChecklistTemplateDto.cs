namespace FusionOS.Modules.Maintenance.Application.ChecklistTemplates.Contracts;

public sealed record ChecklistItemDto(Guid Id, int SequenceNumber, string Description);

public sealed record ChecklistTemplateDto(
    Guid Id,
    string Name,
    string? Description,
    bool IsActive,
    IReadOnlyList<ChecklistItemDto> Items,
    DateTimeOffset CreatedAt);

/// <summary>Single place that turns a ChecklistTemplate aggregate into its DTO, shared by every handler that returns one.</summary>
public static class ChecklistTemplateMapper
{
    public static ChecklistTemplateDto ToDto(Domain.ChecklistTemplates.ChecklistTemplate template) => new(
        template.Id,
        template.Name,
        template.Description,
        template.IsActive,
        template.Items.Select(i => new ChecklistItemDto(i.Id, i.SequenceNumber, i.Description)).ToList(),
        template.CreatedAt);
}
