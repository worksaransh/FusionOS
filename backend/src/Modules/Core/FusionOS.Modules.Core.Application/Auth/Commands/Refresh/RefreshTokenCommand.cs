using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Core.Application.Auth.Contracts;

namespace FusionOS.Modules.Core.Application.Auth.Commands.Refresh;

/// <summary>
/// Exchanges a still-active refresh token for a new access+refresh pair (rotation - 07_SECURITY.md).
/// Implements IPubliclyAccessible (Phase 9 hardening fix, 2026-07-25): identity here comes from
/// possessing the refresh token itself, not staff RBAC — exempt from AuthorizationBehavior's fail-closed check.
/// </summary>
public sealed record RefreshTokenCommand(string RefreshToken) : ICommand<AuthResultDto>, IPubliclyAccessible;
