using FusionOS.Modules.Core.Application.Settings.Commands.UpdateCompanySettings;
using FusionOS.Modules.Core.Application.Settings.Queries.GetCompanySettings;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Core.Api.Controllers;

/// <summary>
/// Per-company Settings (Phase M5, 2026-07-15 — previously 0% per
/// docs/PROJECT_TRACKER.md: no entity, no CQRS, no UI). GET always returns a
/// row — GetCompanySettingsQueryHandler creates sensible defaults on first
/// read rather than requiring a separate bootstrap step.
/// </summary>
[ApiController]
[Route("api/v1/core/settings")]
public sealed class SettingsController : ControllerBase
{
    private readonly ISender _sender;

    public SettingsController(ISender sender) => _sender = sender;

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Get([FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetCompanySettingsQuery(companyId), cancellationToken);
        return Ok(result);
    }

    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Update([FromBody] UpdateCompanySettingsRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateCompanySettingsCommand(request.CompanyId, request.DefaultCurrency, request.DefaultPageSize, request.DisplayName, request.LogoUrl);
        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }
}

public sealed record UpdateCompanySettingsRequest(Guid CompanyId, string DefaultCurrency, int DefaultPageSize, string? DisplayName, string? LogoUrl);
