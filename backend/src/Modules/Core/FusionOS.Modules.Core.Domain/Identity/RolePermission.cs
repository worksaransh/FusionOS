namespace FusionOS.Modules.Core.Domain.Identity;

public sealed class RolePermission
{
    public Guid RoleId { get; private set; }
    public Guid PermissionId { get; private set; }

    private RolePermission() { }

    public static RolePermission Create(Guid roleId, Guid permissionId) => new() { RoleId = roleId, PermissionId = permissionId };
}
