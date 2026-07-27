using FusionOS.BuildingBlocks.Application.Abstractions;

namespace FusionOS.Modules.Sales.Application.PortalUsers.Commands.PortalLogin;

public sealed record PortalLoginResult(Guid PortalUserId, Guid CustomerId, string Email, string AccessToken, DateTimeOffset AccessTokenExpiresAt);

/// <summary>
/// Customer self-service login (email + password only - no CustomerId in the
/// request; see CustomerPortalUser's doc comment on why email is only unique
/// per-Customer, not globally). Deliberately NOT gated by IRequirePermission
/// - there is no signed-in caller yet, same as Core's own LoginCommand.
/// </summary>
public sealed record PortalLoginCommand(string Email, string Password) : ICommand<PortalLoginResult>, IPubliclyAccessible;
