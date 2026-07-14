using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Core.Application.Auth.Contracts;

namespace FusionOS.Modules.Core.Application.Auth.Commands.Login;

/// <summary>
/// Authenticates by email+password. CompanyId is optional: users who belong to
/// exactly one company don't need to pass it; users who belong to several must
/// specify which one to log into (07_SECURITY.md — multi-tenant identity).
/// Not IRequirePermission (nothing is authenticated yet) and not IAuditableCommand
/// (AuditBehavior requires a current user, which does not exist pre-login;
/// login/logout auditing is tracked separately — see README known-gaps).
/// </summary>
public sealed record LoginCommand(string Email, string Password, Guid? CompanyId) : ICommand<AuthResultDto>;
