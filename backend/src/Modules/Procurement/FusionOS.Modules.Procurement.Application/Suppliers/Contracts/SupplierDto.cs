namespace FusionOS.Modules.Procurement.Application.Suppliers.Contracts;

public sealed record SupplierDto(Guid Id, string Name, string Code, string? ContactEmail, string? ContactPhone, bool IsActive, DateTimeOffset CreatedAt);
