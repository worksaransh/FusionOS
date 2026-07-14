using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Core.Application.Auth.Contracts;

namespace FusionOS.Modules.Core.Application.Auth.Commands.Register;

/// <summary>
/// Creates a User and links them into a Company. Scope note (documented plainly
/// rather than hidden, per the audit's own standard): this endpoint always grants
/// the company's bootstrap "Owner" role (created on first use, with every known
/// Permission attached). Fine-grained "invite a teammate with a specific limited
/// role" is a distinct RBAC-administration feature, not yet built - see README.
/// </summary>
public sealed record RegisterUserCommand(string Email, string FullName, string Password, Guid CompanyId)
    : ICommand<UserDto>, IAuditableCommand
{
    public string EntityType => "User";
    public Guid EntityId { get; init; }
    public string Action => "Registered";
}
