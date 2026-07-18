using FusionOS.Modules.IntegrationHub.Application.ConnectorConnections.Commands.ConnectConnector;
using FusionOS.Modules.IntegrationHub.Application.ConnectorConnections.Commands.DisconnectConnector;
using FusionOS.Modules.IntegrationHub.Application.ConnectorConnections.Commands.MarkConnectorConnectionError;
using FusionOS.Modules.IntegrationHub.Application.ConnectorConnections.Queries.ListConnectorConnections;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.IntegrationHub.Api.Controllers;

/// <summary>
/// Phase 9 — Integration Hub: a company's connections to catalog connectors, Connected →
/// Disconnected, or flagged Error. No GetById — no controller action needs to fetch a
/// single connection, only list, same simplification Marketplace's
/// PluginInstallationsController makes.
/// </summary>
[ApiController]
[Route("api/v1/integration-hub/connections")]
public sealed class ConnectorConnectionsController : ControllerBase
{
    private readonly ISender _sender;

    public ConnectorConnectionsController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Connect([FromBody] ConnectConnectorRequest request, CancellationToken cancellationToken)
    {
        var command = new ConnectConnectorCommand(request.CompanyId, request.IntegrationConnectorId, request.Label);
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(List), new { companyId = request.CompanyId }, result);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] Guid companyId, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListConnectorConnectionsQuery(companyId, page, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/disconnect")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Disconnect(Guid id, [FromBody] ConnectorConnectionActionRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DisconnectConnectorCommand(request.CompanyId, id), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/mark-error")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkError(Guid id, [FromBody] ConnectorConnectionActionRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new MarkConnectorConnectionErrorCommand(request.CompanyId, id), cancellationToken);
        return Ok(result);
    }
}

public sealed record ConnectConnectorRequest(Guid CompanyId, Guid IntegrationConnectorId, string Label);

public sealed record ConnectorConnectionActionRequest(Guid CompanyId);
