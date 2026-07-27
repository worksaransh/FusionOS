using FusionOS.Modules.Ai.Application.OcrJobs.Commands.CreateDraftFromOcrExtraction;
using FusionOS.Modules.Ai.Application.OcrJobs.Commands.SubmitOcrJob;
using FusionOS.Modules.Ai.Application.OcrJobs.Queries.GetOcrJobById;
using FusionOS.Modules.Ai.Application.OcrJobs.Queries.ListOcrJobs;
using FusionOS.Modules.Ai.Domain.OcrJobs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Ai.Api.Controllers;

/// <summary>
/// Phase 8 — AI &amp; Integrations: OCR/invoice-processing job tracking,
/// `docs/blueprint/12_AI_PLATFORM.md` §4. Submitting a job runs synchronously against
/// whatever <c>IOcrProvider</c> is registered (today: <c>UnavailableOcrProvider</c>, which
/// always fails with an honest, actionable reason — see that class's doc comment). Nothing
/// here auto-posts to Procurement/Finance; <see cref="CreateDraft"/> only records a
/// bookkeeping link once a job is Completed, which requires a real provider to be wired in.
/// </summary>
[ApiController]
[Route("api/v1/ai/ocr-jobs")]
public sealed class OcrJobsController : ControllerBase
{
    private readonly ISender _sender;

    public OcrJobsController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Submit([FromBody] SubmitOcrJobRequest request, CancellationToken cancellationToken)
    {
        var command = new SubmitOcrJobCommand(request.CompanyId, request.DocumentId);
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id, companyId = request.CompanyId }, result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetOcrJobByIdQuery(companyId, id), cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] Guid companyId, [FromQuery] OcrJobStatus? status = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListOcrJobsQuery(companyId, status, page, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/create-draft")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateDraft(Guid id, [FromBody] CreateDraftFromOcrExtractionRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateDraftFromOcrExtractionCommand(request.CompanyId, id, request.TargetReferenceType, request.TargetReferenceId);
        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }
}

public sealed record SubmitOcrJobRequest(Guid CompanyId, Guid DocumentId);

public sealed record CreateDraftFromOcrExtractionRequest(Guid CompanyId, string TargetReferenceType, Guid TargetReferenceId);
