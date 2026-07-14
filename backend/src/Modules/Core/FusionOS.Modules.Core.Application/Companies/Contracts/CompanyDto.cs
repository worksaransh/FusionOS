namespace FusionOS.Modules.Core.Application.Companies.Contracts;

/// <summary>Published DTO — the only shape other modules or clients depend on (03_SYSTEM_ARCHITECTURE.md §2).</summary>
public sealed record CompanyDto(Guid Id, string Name, string LegalName, string? TaxId, string BaseCurrency, bool IsActive, DateTimeOffset CreatedAt);
