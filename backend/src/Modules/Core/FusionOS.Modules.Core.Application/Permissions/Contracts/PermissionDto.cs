namespace FusionOS.Modules.Core.Application.Permissions.Contracts;

/// <summary>One entry from the static PermissionCatalog, shaped for the RBAC admin UI to render as a checkbox list.</summary>
public sealed record PermissionDto(string Module, string Code, string Description);
