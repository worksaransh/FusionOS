namespace FusionOS.Modules.Core.Application.Departments.Contracts;

/// <summary>Published DTO — the only shape other modules or clients depend on (03_SYSTEM_ARCHITECTURE.md §2).</summary>
public sealed record DepartmentDto(Guid Id, Guid CompanyId, Guid? BranchId, string Name, string Code, Guid? ParentDepartmentId, bool IsActive, DateTimeOffset CreatedAt);
