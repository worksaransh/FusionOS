namespace FusionOS.Modules.Inventory.Application.Attributes.Contracts;

public sealed record AttributeValueDto(Guid Id, Guid AttributeDefinitionId, string Value, bool IsActive, DateTimeOffset CreatedAt);
