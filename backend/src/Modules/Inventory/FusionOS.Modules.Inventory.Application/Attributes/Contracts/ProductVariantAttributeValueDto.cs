namespace FusionOS.Modules.Inventory.Application.Attributes.Contracts;

/// <summary>Read shape for one variant-attribute assignment — carries both ids (for cache keys / follow-up calls) and resolved display names (so the frontend doesn't need a second round-trip per row).</summary>
public sealed record ProductVariantAttributeValueDto(
    Guid Id,
    Guid ProductId,
    Guid VariantId,
    Guid AttributeDefinitionId,
    string AttributeDefinitionName,
    Guid AttributeValueId,
    string AttributeValue);
