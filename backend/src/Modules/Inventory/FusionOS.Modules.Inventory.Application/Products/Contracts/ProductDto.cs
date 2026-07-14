namespace FusionOS.Modules.Inventory.Application.Products.Contracts;

/// <summary>Published DTO — the only shape other modules (Warehouse, Procurement, Sales) will depend on once they reference products (03_SYSTEM_ARCHITECTURE.md §2).</summary>
public sealed record ProductDto(Guid Id, string Sku, string Name, string? Description, string UnitOfMeasure, bool IsActive, DateTimeOffset CreatedAt);
