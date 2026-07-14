using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Core.Application.Auth.Contracts;

namespace FusionOS.Modules.Core.Application.Auth.Commands.Refresh;

/// <summary>Exchanges a still-active refresh token for a new access+refresh pair (rotation - 07_SECURITY.md).</summary>
public sealed record RefreshTokenCommand(string RefreshToken) : ICommand<AuthResultDto>;
