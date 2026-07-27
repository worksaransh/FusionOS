using FusionOS.Modules.Crm.Application.Tags.Commands.AttachTagToEntity;
using FusionOS.Modules.Crm.Application.Tags.Commands.CreateTag;
using FusionOS.Modules.Crm.Application.Tags.Commands.DetachTagFromEntity;
using FusionOS.Modules.Crm.Application.Tags.Queries.ListTags;
using FusionOS.Modules.Crm.Application.Tags.Queries.ListTagsForEntity;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Crm.Api.Controllers;

/// <summary>Phase 7 — CRM depth: customer segmentation/tagging across Leads/Contacts/Accounts/Opportunities.</summary>
[ApiController]
[Route("api/v1/crm/tags")]
public sealed class TagsController : ControllerBase
{
    private readonly ISender _sender;

    public TagsController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateTagRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new CreateTagCommand(request.CompanyId, request.Name, request.Color), cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] Guid companyId, [FromQuery] string? search = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListTagsQuery(companyId, search, page, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpGet("for-entity")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ListForEntity([FromQuery] Guid companyId, [FromQuery] string entityType, [FromQuery] Guid entityId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ListTagsForEntityQuery(companyId, entityType, entityId), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/attach")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Attach(Guid id, [FromBody] TagEntityRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new AttachTagToEntityCommand(request.CompanyId, id, request.EntityType, request.EntityId), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/detach")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Detach(Guid id, [FromBody] TagEntityRequest request, CancellationToken cancellationToken)
    {
        await _sender.Send(new DetachTagFromEntityCommand(request.CompanyId, id, request.EntityType, request.EntityId), cancellationToken);
        return NoContent();
    }
}

public sealed record CreateTagRequest(Guid CompanyId, string Name, string? Color);

public sealed record TagEntityRequest(Guid CompanyId, string EntityType, Guid EntityId);
