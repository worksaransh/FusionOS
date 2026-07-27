using FusionOS.Modules.Inventory.Application.Attributes.Commands.CreateAttributeDefinition;
using FusionOS.Modules.Inventory.Application.Attributes.Commands.CreateAttributeValue;
using FusionOS.Modules.Inventory.Application.Attributes.Commands.DeactivateAttributeDefinition;
using FusionOS.Modules.Inventory.Application.Attributes.Commands.DeactivateAttributeValue;
using FusionOS.Modules.Inventory.Application.Attributes.Commands.UpdateAttributeDefinition;
using FusionOS.Modules.Inventory.Application.Attributes.Queries.GetAttributeDefinitionById;
using FusionOS.Modules.Inventory.Application.Attributes.Queries.ListAttributeDefinitions;
using FusionOS.Modules.Inventory.Application.Attributes.Queries.ListAttributeValuesByDefinition;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Inventory.Api.Controllers;

/// <summary>
/// Reusable, controlled-vocabulary product attribute schema (e.g. "Color" ->
/// Red/Blue/Green) — the structured counterpart to ProductVariant's existing
/// free-text Attributes string (see ProductVariant.cs and AttributeDefinition.cs
/// doc comments). Same master-data CRUD shape as CostCentersController; values
/// are nested under their owning definition (.../{id}/values), same "nested
/// resource" convention as ProductsController's variants.
/// </summary>
[ApiController]
[Route("api/v1/inventory/attribute-definitions")]
public sealed class AttributeDefinitionsController : ControllerBase
{
    private readonly ISender _sender;

    public AttributeDefinitionsController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateAttributeDefinitionRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateAttributeDefinitionCommand(request.CompanyId, request.Name);
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id, companyId = request.CompanyId }, result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetAttributeDefinitionByIdQuery(companyId, id), cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] Guid companyId, [FromQuery] string? search = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListAttributeDefinitionsQuery(companyId, search, page, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAttributeDefinitionRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateAttributeDefinitionCommand(request.CompanyId, id, request.Name);
        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }

    // Soft-deactivate only — never a DELETE (04_DATABASE_GUIDELINES.md).
    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(Guid id, [FromBody] DeactivateAttributeDefinitionRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeactivateAttributeDefinitionCommand(request.CompanyId, id), cancellationToken);
        return Ok(result);
    }

    // --- Nested AttributeValue CRUD (Create/Deactivate/List-by-definition only — see the task's scope for AttributeValue) ---

    [HttpPost("{id:guid}/values")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateValue(Guid id, [FromBody] CreateAttributeValueRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateAttributeValueCommand(request.CompanyId, id, request.Value);
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(ListValues), new { id, companyId = request.CompanyId }, result);
    }

    [HttpGet("{id:guid}/values")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ListValues(Guid id, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ListAttributeValuesByDefinitionQuery(companyId, id), cancellationToken);
        return Ok(result);
    }

    // Soft-deactivate only — same convention as the definition itself.
    [HttpPost("{id:guid}/values/{valueId:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeactivateValue(Guid id, Guid valueId, [FromBody] DeactivateAttributeValueRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeactivateAttributeValueCommand(request.CompanyId, valueId), cancellationToken);
        return Ok(result);
    }
}

public sealed record CreateAttributeDefinitionRequest(Guid CompanyId, string Name);

public sealed record UpdateAttributeDefinitionRequest(Guid CompanyId, string Name);

public sealed record DeactivateAttributeDefinitionRequest(Guid CompanyId);

public sealed record CreateAttributeValueRequest(Guid CompanyId, string Value);

public sealed record DeactivateAttributeValueRequest(Guid CompanyId);
