using FusionOS.Modules.Marketplace.Application.PluginInstallations.Commands.DisablePluginInstallation;
using FusionOS.Modules.Marketplace.Application.PluginInstallations.Commands.EnablePluginInstallation;
using FusionOS.Modules.Marketplace.Application.PluginInstallations.Commands.InstallPlugin;
using FusionOS.Modules.Marketplace.Application.PluginInstallations.Commands.UninstallPlugin;
using FusionOS.Modules.Marketplace.Application.PluginInstallations.Queries.ListPluginInstallations;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Marketplace.Api.Controllers;

/// <summary>
/// Phase 8 — Marketplace: a company's installs of catalog listings,
/// Installed → Disabled/Uninstalled. No GetById — no controller action needs to fetch a
/// single installation, only list, same simplification BusinessIntelligence's
/// KpiSnapshotsController makes.
/// </summary>
[ApiController]
[Route("api/v1/marketplace/plugin-installations")]
public sealed class PluginInstallationsController : ControllerBase
{
    private readonly ISender _sender;

    public PluginInstallationsController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Install([FromBody] InstallPluginRequest request, CancellationToken cancellationToken)
    {
        var command = new InstallPluginCommand(request.CompanyId, request.PluginListingId);
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(List), new { companyId = request.CompanyId }, result);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] Guid companyId, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListPluginInstallationsQuery(companyId, page, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/disable")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Disable(Guid id, [FromBody] PluginInstallationActionRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DisablePluginInstallationCommand(request.CompanyId, id), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/enable")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Enable(Guid id, [FromBody] PluginInstallationActionRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new EnablePluginInstallationCommand(request.CompanyId, id), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/uninstall")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Uninstall(Guid id, [FromBody] PluginInstallationActionRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new UninstallPluginCommand(request.CompanyId, id), cancellationToken);
        return Ok(result);
    }
}

public sealed record InstallPluginRequest(Guid CompanyId, Guid PluginListingId);

public sealed record PluginInstallationActionRequest(Guid CompanyId);
