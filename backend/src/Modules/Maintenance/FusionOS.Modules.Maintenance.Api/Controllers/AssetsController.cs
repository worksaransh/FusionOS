using FusionOS.Modules.Maintenance.Application.Assets.Commands.CreateAsset;
using FusionOS.Modules.Maintenance.Application.Assets.Commands.DeactivateAsset;
using FusionOS.Modules.Maintenance.Application.Assets.Queries.GetAssetById;
using FusionOS.Modules.Maintenance.Application.Assets.Queries.ListAssets;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Maintenance.Api.Controllers;

/// <summary>
/// Phase 5 — Maintenance: the machine register. CRUD-ish shape (create/read/list/
/// soft-deactivate) mirroring CostCentersController.
/// </summary>
[ApiController]
[Route("api/v1/maintenance/assets")]
public sealed class AssetsController : ControllerBase
{
    private readonly ISender _sender;

    public AssetsController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateAssetRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateAssetCommand(request.CompanyId, request.Code, request.Name, request.Location);
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id, companyId = request.CompanyId }, result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetAssetByIdQuery(companyId, id), cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] Guid companyId, [FromQuery] string? search = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListAssetsQuery(companyId, search, page, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(Guid id, [FromBody] DeactivateAssetRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeactivateAssetCommand(request.CompanyId, id), cancellationToken);
        return Ok(result);
    }
}

public sealed record CreateAssetRequest(Guid CompanyId, string Code, string Name, string? Location);

public sealed record DeactivateAssetRequest(Guid CompanyId);
