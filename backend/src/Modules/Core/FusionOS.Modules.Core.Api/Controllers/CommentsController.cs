using FusionOS.Modules.Core.Application.Comments.Commands.CreateComment;
using FusionOS.Modules.Core.Application.Comments.Commands.DeleteComment;
using FusionOS.Modules.Core.Application.Comments.Commands.UpdateComment;
using FusionOS.Modules.Core.Application.Comments.Queries.ListCommentsByEntity;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Core.Api.Controllers;

/// <summary>
/// User-authored comments on any (EntityType, EntityId) target — net-new,
/// alongside the pre-existing system-generated AuditLog (AuditLogController).
/// See FusionOS.Modules.Core.Domain.Comments.Comment's doc comment for the
/// author-only-edit / author-or-moderator-delete authorization split.
/// </summary>
[ApiController]
[Route("api/v1/core/comments")]
public sealed class CommentsController : ControllerBase
{
    private readonly ISender _sender;

    public CommentsController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateCommentCommand command, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(List), new { companyId = command.CompanyId, entityType = command.EntityType, entityId = command.EntityId }, result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCommentRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new UpdateCommentCommand(request.CompanyId, id, request.Body), cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteCommentCommand(companyId, id), cancellationToken);
        return NoContent();
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] Guid companyId, [FromQuery] string entityType, [FromQuery] Guid entityId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ListCommentsByEntityQuery(companyId, entityType, entityId), cancellationToken);
        return Ok(result);
    }
}

public sealed record UpdateCommentRequest(Guid CompanyId, string Body);
