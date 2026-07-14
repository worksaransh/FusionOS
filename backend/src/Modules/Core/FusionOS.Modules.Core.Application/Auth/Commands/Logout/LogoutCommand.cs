using FusionOS.BuildingBlocks.Application.Abstractions;

namespace FusionOS.Modules.Core.Application.Auth.Commands.Logout;

/// <summary>Revokes a single refresh token (e.g. "sign out this device"). Idempotent - an already-revoked or unknown token is a no-op.</summary>
public sealed record LogoutCommand(string RefreshToken) : ICommand;
