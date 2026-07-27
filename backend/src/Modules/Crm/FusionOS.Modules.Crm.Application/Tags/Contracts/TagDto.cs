namespace FusionOS.Modules.Crm.Application.Tags.Contracts;

public sealed record TagDto(Guid Id, string Name, string? Color);

/// <summary>Single place that turns a Tag aggregate into its DTO, shared by every handler that returns one.</summary>
public static class TagMapper
{
    public static TagDto ToDto(Domain.Tags.Tag tag) => new(tag.Id, tag.Name, tag.Color);
}
