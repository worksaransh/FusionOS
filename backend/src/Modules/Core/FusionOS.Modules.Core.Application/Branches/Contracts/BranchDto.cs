namespace FusionOS.Modules.Core.Application.Branches.Contracts;

/// <summary>Published DTO — the only shape other modules or clients depend on (03_SYSTEM_ARCHITECTURE.md §2).</summary>
public sealed record BranchDto(Guid Id, Guid CompanyId, string Name, string Code, bool IsHeadOffice, bool IsActive, DateTimeOffset CreatedAt);
