using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Core.Application.Auth.Contracts;

namespace FusionOS.Modules.Core.Application.Auth.Commands.Register;

/// <summary>
/// Creates a User and links them into a Company. Scope note (Phase H3,
/// 2026-07-14 sprint audit - documented plainly rather than hidden, per the
/// audit's own standard): only a brand-new company's very first user gets the
/// bootstrap "Owner" role (created on first use, with every known Permission
/// attached). Every registration after that is an "invite a teammate" action,
/// gated by core.user.register, and instead lands on the zero-permission
/// "Member" role - an existing Owner promotes them afterward via the RBAC
/// administration UI (RolesPage -> SetRolePermissionsCommand /
/// AssignUserRoleCommand). See RegisterUserCommandHandler for the exact rule.
/// </summary>
public sealed record RegisterUserCommand(string Email, string FullName, string Password, Guid CompanyId)
    : ICommand<UserDto>, IAuditableCommand
{
    public string EntityType => "User";
    public Guid EntityId { get; init; }
    public string Action => "Registered";
}
