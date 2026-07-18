using FusionOS.Modules.Core.Application.Users.Commands.AssignUserRole;
using FusionOS.Modules.Core.Application.Users.Queries.ListCompanyUsers;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Core.Api.Controllers;

/// <summary>
/// Company user roster + role assignment (2026-07-14 sprint audit, Phase H2).
/// Registration itself stays on AuthController — this is the admin-facing
/// "who has which role" view and the action that changes it.
/// </summary>
[ApiController]
[Route("api/v1/core/users")]
public sealed class UsersController : ControllerBase
{
    private readonly ISender _sender;

    public UsersController(ISender sender) => _sender = sender;

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] Guid companyId, [FromQuery] string? search = null, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListCompanyUsersQuery(companyId, search), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{userId:guid}/role")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignRole(Guid userId, [FromBody] AssignUserRoleRequest request, CancellationToken cancellationToken)
    {
        await _sender.Send(new AssignUserRoleCommand(request.CompanyId, userId, request.RoleId), cancellationToken);
        return NoContent();
    }
}

/// <summary>Body shape for POST .../role — UserId comes from the route, not the body.</summary>
public sealed record AssignUserRoleRequest(Guid CompanyId, Guid RoleId);
