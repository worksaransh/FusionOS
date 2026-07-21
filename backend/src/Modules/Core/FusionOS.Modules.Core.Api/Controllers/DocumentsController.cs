using FusionOS.Modules.Core.Application.Documents.Commands.DeleteDocument;
using FusionOS.Modules.Core.Application.Documents.Commands.UploadDocument;
using FusionOS.Modules.Core.Application.Documents.Queries.DownloadDocument;
using FusionOS.Modules.Core.Application.Documents.Queries.ListDocumentsByEntity;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Core.Api.Controllers;

/// <summary>
/// Generic file-attachment subsystem (net-new, 2026-07-21) — see
/// FusionOS.Modules.Core.Domain.Documents.Document's doc comment for the
/// polymorphic-reference convention (borrowed from ApprovalRequest) and the
/// bytea-in-Postgres storage decision. Any module's page can attach files to
/// any of its records by passing that record's own (EntityType, EntityId).
/// </summary>
[ApiController]
[Route("api/v1/core/documents")]
public sealed class DocumentsController : ControllerBase
{
    private readonly ISender _sender;

    public DocumentsController(ISender sender) => _sender = sender;

    [HttpPost]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Upload(
        [FromForm] Guid companyId,
        [FromForm] string entityType,
        [FromForm] Guid entityId,
        [FromForm] IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
            return BadRequest("A non-empty file is required.");

        await using var stream = new MemoryStream();
        await file.CopyToAsync(stream, cancellationToken);

        var command = new UploadDocumentCommand(companyId, entityType, entityId, file.FileName, file.ContentType, stream.ToArray());
        var result = await _sender.Send(command, cancellationToken);

        return CreatedAtAction(nameof(List), new { companyId, entityType = result.EntityType, entityId = result.EntityId }, result);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> List(
        [FromQuery] Guid companyId,
        [FromQuery] string entityType,
        [FromQuery] Guid entityId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListDocumentsByEntityQuery(companyId, entityType, entityId, page, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}/download")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Download(Guid id, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DownloadDocumentQuery(companyId, id), cancellationToken);
        return File(result.Content, result.ContentType, result.FileName);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteDocumentCommand(companyId, id), cancellationToken);
        return NoContent();
    }
}
