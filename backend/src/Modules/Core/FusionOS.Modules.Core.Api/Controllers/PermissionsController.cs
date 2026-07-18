using FusionOS.Modules.Core.Application.Permissions.Queries.ListPermissions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Core.Api.Controllers;

/// <summary>Read-only permission catalog for the RBAC admin UI (2026-07-14 sprint audit, Phase H2).</summary>
[ApiController]
[Route("api/v1/core/permissions")]
public sealed class PermissionsController : ControllerBase
{
    private readonly ISender _sender;

    public PermissionsController(ISender sender) => _sender = sender;

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] string? search = null, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListPermissionsQuery(search), cancellationToken);
        return Ok(result);
    }
}
