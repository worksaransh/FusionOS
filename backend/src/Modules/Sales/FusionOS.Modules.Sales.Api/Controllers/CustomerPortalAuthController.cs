using FusionOS.Modules.Sales.Application.PortalUsers.Commands.PortalLogin;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Sales.Api.Controllers;

/// <summary>
/// Customer self-service login (Customer Portal, 2026-07-22). The only
/// anonymous route in this whole slice - every other portal endpoint requires
/// a valid "CustomerPortal"-policy JWT (see PortalController and the
/// AddAuthorization policy registered in Program.cs).
/// </summary>
[ApiController]
[Route("api/v1/sales/portal/auth")]
[AllowAnonymous]
public sealed class CustomerPortalAuthController : ControllerBase
{
    private readonly ISender _sender;

    public CustomerPortalAuthController(ISender sender) => _sender = sender;

    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] PortalLoginRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new PortalLoginCommand(request.Email, request.Password), cancellationToken);
        return Ok(result);
    }
}

public sealed record PortalLoginRequest(string Email, string Password);
