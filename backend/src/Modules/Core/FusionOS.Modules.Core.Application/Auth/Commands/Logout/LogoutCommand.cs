using FusionOS.BuildingBlocks.Application.Abstractions;

namespace FusionOS.Modules.Core.Application.Auth.Commands.Logout;

/// <summary>
/// Revokes a single refresh token (e.g. "sign out this device"). Idempotent - an already-revoked or unknown token is a no-op.
/// Implements IPubliclyAccessible (Phase 9 hardening fix, 2026-07-25): identity here comes from
/// possessing the refresh token itself, not staff RBAC — exempt from AuthorizationBehavior's fail-closed check.
/// </summary>
public sealed record LogoutCommand(string RefreshToken) : ICommand, IPubliclyAccessible;
