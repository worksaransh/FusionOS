using FusionOS.Modules.Marketplace.Application.PluginListings.Commands.CreatePluginListing;
using FusionOS.Modules.Marketplace.Application.PluginListings.Commands.DeactivatePluginListing;
using FusionOS.Modules.Marketplace.Application.PluginListings.Queries.GetPluginListingById;
using FusionOS.Modules.Marketplace.Application.PluginListings.Queries.ListPluginListings;
using FusionOS.Modules.Marketplace.Domain.PluginListings;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Marketplace.Api.Controllers;

/// <summary>
/// Phase 8 — Marketplace: the extension catalog. CRUD-ish shape (create/read/list/
/// soft-deactivate) mirroring CostCentersController/AssetsController/KpiDefinitionsController.
/// </summary>
[ApiController]
[Route("api/v1/marketplace/plugin-listings")]
public sealed class PluginListingsController : ControllerBase
{
    private readonly ISender _sender;

    public PluginListingsController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreatePluginListingRequest request, CancellationToken cancellationToken)
    {
        var command = new CreatePluginListingCommand(request.CompanyId, request.Code, request.Name, request.Publisher, request.Category);
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id, companyId = request.CompanyId }, result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetPluginListingByIdQuery(companyId, id), cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] Guid companyId, [FromQuery] string? search = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListPluginListingsQuery(companyId, search, page, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(Guid id, [FromBody] DeactivatePluginListingRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeactivatePluginListingCommand(request.CompanyId, id), cancellationToken);
        return Ok(result);
    }
}

public sealed record CreatePluginListingRequest(Guid CompanyId, string Code, string Name, string Publisher, PluginCategory Category);

public sealed record DeactivatePluginListingRequest(Guid CompanyId);
