using FusionOS.Modules.IntegrationHub.Application.ConnectorConnections.Commands.ConfigureConnectorCredential;
using FusionOS.Modules.IntegrationHub.Application.ConnectorConnections.Commands.ConnectConnector;
using FusionOS.Modules.IntegrationHub.Application.ConnectorConnections.Commands.DisconnectConnector;
using FusionOS.Modules.IntegrationHub.Application.ConnectorConnections.Commands.MarkConnectorConnectionError;
using FusionOS.Modules.IntegrationHub.Application.ConnectorConnections.Queries.ListConnectorConnections;
using FusionOS.Modules.IntegrationHub.Application.ConnectorSyncRuns.Commands.TriggerConnectorSync;
using FusionOS.Modules.IntegrationHub.Application.ConnectorSyncRuns.Queries.ListConnectorSyncRuns;
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

    /// <summary>
    /// Sets the connection's credential reference — a config/environment key NAME
    /// (e.g. "SHOPIFY_STORE_ACME_API_KEY"), never a secret value. No dedicated GET
    /// .../credential endpoint: the reference string is already returned as part of
    /// every ConnectorConnectionDto (Connect/List/Disconnect/MarkError all include it),
    /// and "integration_hub.credential.read" gates that same field there — a separate
    /// read action would be a redundant route for data already on the list response.
    /// </summary>
    [HttpPost("{id:guid}/credential")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ConfigureCredential(Guid id, [FromBody] ConfigureConnectorCredentialRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ConfigureConnectorCredentialCommand(request.CompanyId, id, request.CredentialReference), cancellationToken);
        return Ok(result);
    }

    /// <summary>Manually runs one sync attempt through whichever IConnectorSyncAdapter matches this connection's connector Provider — see TriggerConnectorSyncCommandHandler.</summary>
    [HttpPost("{id:guid}/sync")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> TriggerSync(Guid id, [FromBody] ConnectorConnectionActionRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new TriggerConnectorSyncCommand(request.CompanyId, id), cancellationToken);
        return Ok(result);
    }

    /// <summary>Sync run history for one connection — most recent first.</summary>
    [HttpGet("{id:guid}/sync-runs")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ListSyncRuns(Guid id, [FromQuery] Guid companyId, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListConnectorSyncRunsQuery(companyId, id, page, pageSize), cancellationToken);
        return Ok(result);
    }
}

public sealed record ConnectConnectorRequest(Guid CompanyId, Guid IntegrationConnectorId, string Label);

public sealed record ConnectorConnectionActionRequest(Guid CompanyId);

public sealed record ConfigureConnectorCredentialRequest(Guid CompanyId, string CredentialReference);
