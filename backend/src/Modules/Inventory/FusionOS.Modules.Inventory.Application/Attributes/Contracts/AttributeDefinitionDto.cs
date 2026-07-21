namespace FusionOS.Modules.Inventory.Application.Attributes.Contracts;

public sealed record AttributeDefinitionDto(Guid Id, string Name, bool IsActive, DateTimeOffset CreatedAt);
