using FusionOS.Modules.Core.Application.Roles.Commands.CreateRole;
using FusionOS.Modules.Core.Application.Roles.Commands.SetRolePermissions;
using FusionOS.Modules.Core.Application.Roles.Commands.UpdateRole;
using FusionOS.Modules.Core.Application.Roles.Queries.GetRolePermissions;
using FusionOS.Modules.Core.Application.Roles.Queries.ListRoles;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Core.Api.Controllers;

/// <summary>
/// RBAC administration (2026-07-14 sprint audit, Phase H2) — every endpoint
/// requires "core.role.manage" (enforced in the command/query itself via
/// IRequirePermission, not here). This is what makes the read/write
/// permission gates added across the rest of the API mean something beyond
/// the single auto-granted "Owner" role every company starts with.
/// </summary>
[ApiController]
[Route("api/v1/core/roles")]
public sealed class RolesController : ControllerBase
{
    private readonly ISender _sender;

    public RolesController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateRoleCommand command, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetPermissions), new { id = result.Id, companyId = command.CompanyId }, result);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] Guid companyId, [FromQuery] string? search = null, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListRolesQuery(companyId, search), cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRoleRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new UpdateRoleCommand(request.CompanyId, id, request.Name), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}/permissions")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPermissions(Guid id, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetRolePermissionsQuery(companyId, id), cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:guid}/permissions")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetPermissions(Guid id, [FromBody] SetRolePermissionsRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new SetRolePermissionsCommand(request.CompanyId, id, request.PermissionCodes), cancellationToken);
        return Ok(result);
    }
}

/// <summary>Body shape for PUT .../permissions — RoleId comes from the route, not the body.</summary>
public sealed record SetRolePermissionsRequest(Guid CompanyId, IReadOnlyList<string> PermissionCodes);

/// <summary>Body shape for PUT /{id} — RoleId comes from the route, not the body.</summary>
public sealed record UpdateRoleRequest(Guid CompanyId, string Name);
