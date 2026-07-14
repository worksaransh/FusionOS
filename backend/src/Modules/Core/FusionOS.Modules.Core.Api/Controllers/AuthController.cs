using FusionOS.Modules.Core.Application.Auth.Commands.Login;
using FusionOS.Modules.Core.Application.Auth.Commands.Logout;
using FusionOS.Modules.Core.Application.Auth.Commands.Refresh;
using FusionOS.Modules.Core.Application.Auth.Commands.Register;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Core.Api.Controllers;

/// <summary>
/// Authentication endpoints (07_SECURITY.md). Login/refresh/logout are
/// [AllowAnonymous] because AuthorizationExtensions.AddAuthorizationWithFallbackPolicy
/// (Host Program.cs) makes every OTHER endpoint in the API require a signed-in
/// user by default - these three are the deliberate, narrow exception.
/// Register requires being signed in EXCEPT for a brand-new company's first
/// user; that data-dependent rule is enforced inside RegisterUserCommandHandler,
/// not via an attribute, so this action is also [AllowAnonymous].
/// </summary>
[ApiController]
[Route("api/v1/core/auth")]
[AllowAnonymous]
public sealed class AuthController : ControllerBase
{
    private readonly ISender _sender;

    public AuthController(ISender sender) => _sender = sender;

    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginCommand command, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpPost("refresh")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenCommand command, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout([FromBody] LogoutCommand command, CancellationToken cancellationToken)
    {
        await _sender.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Register([FromBody] RegisterUserCommand command, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(Login), null, result);
    }
}
