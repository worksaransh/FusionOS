namespace FusionOS.Modules.Core.Application.Roles.Contracts;

/// <summary>Published DTO — the only shape other layers or clients depend on (03_SYSTEM_ARCHITECTURE.md §2).</summary>
public sealed record RoleDto(Guid Id, string Name, bool IsSystemRole);
