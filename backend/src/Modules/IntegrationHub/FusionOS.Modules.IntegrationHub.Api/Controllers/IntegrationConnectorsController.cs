using FusionOS.Modules.IntegrationHub.Application.IntegrationConnectors.Commands.CreateIntegrationConnector;
using FusionOS.Modules.IntegrationHub.Application.IntegrationConnectors.Commands.DeactivateIntegrationConnector;
using FusionOS.Modules.IntegrationHub.Application.IntegrationConnectors.Queries.GetIntegrationConnectorById;
using FusionOS.Modules.IntegrationHub.Application.IntegrationConnectors.Queries.ListIntegrationConnectors;
using FusionOS.Modules.IntegrationHub.Domain.IntegrationConnectors;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.IntegrationHub.Api.Controllers;

/// <summary>
/// Phase 9 — Integration Hub: the connector catalog. CRUD-ish shape (create/read/list/
/// soft-deactivate) mirroring PluginListingsController/KpiDefinitionsController.
/// </summary>
[ApiController]
[Route("api/v1/integration-hub/connectors")]
public sealed class IntegrationConnectorsController : ControllerBase
{
    private readonly ISender _sender;

    public IntegrationConnectorsController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateIntegrationConnectorRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateIntegrationConnectorCommand(request.CompanyId, request.Code, request.Name, request.Provider, request.Category);
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id, companyId = request.CompanyId }, result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetIntegrationConnectorByIdQuery(companyId, id), cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] Guid companyId, [FromQuery] string? search = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListIntegrationConnectorsQuery(companyId, search, page, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(Guid id, [FromBody] DeactivateIntegrationConnectorRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeactivateIntegrationConnectorCommand(request.CompanyId, id), cancellationToken);
        return Ok(result);
    }
}

public sealed record CreateIntegrationConnectorRequest(Guid CompanyId, string Code, string Name, string Provider, ConnectorCategory Category);

public sealed record DeactivateIntegrationConnectorRequest(Guid CompanyId);
