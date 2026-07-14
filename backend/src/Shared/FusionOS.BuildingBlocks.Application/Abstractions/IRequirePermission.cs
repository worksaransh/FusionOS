namespace FusionOS.BuildingBlocks.Application.Abstractions;

/// <summary>
/// Implemented by commands/queries that must be authorized before the handler runs.
/// Enforced by AuthorizationBehavior — every module opts in explicitly per
/// 07_SECURITY.md §2 rather than relying on a default-allow posture.
/// </summary>
public interface IRequirePermission
{
    string[] RequiredPermissions { get; }
}
