using FusionOS.Modules.Ai.Application.Copilot.Commands.SendCopilotMessage;
using FusionOS.Modules.Ai.Application.Copilot.Commands.StartCopilotSession;
using FusionOS.Modules.Ai.Application.Copilot.Queries.GetCopilotSessionById;
using FusionOS.Modules.Ai.Application.Copilot.Queries.ListCopilotSessionsForUser;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Ai.Api.Controllers;

/// <summary>
/// Phase 8 — AI Copilot (`docs/blueprint/12_AI_PLATFORM.md` §4): the conversational surface,
/// a client of the same MediatR-dispatched, RBAC-gated queries any other client uses. See
/// CopilotSession's own class doc comment for what this persists, and
/// SendCopilotMessageCommandHandler for the retrieval + response-generation flow.
/// </summary>
[ApiController]
[Route("api/v1/ai/copilot")]
public sealed class CopilotController : ControllerBase
{
    private readonly ISender _sender;

    public CopilotController(ISender sender) => _sender = sender;

    [HttpPost("sessions")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> StartSession([FromBody] StartCopilotSessionRequest request, CancellationToken cancellationToken)
    {
        var command = new StartCopilotSessionCommand(request.CompanyId, request.UserId, request.Title);
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id, companyId = request.CompanyId }, result);
    }

    [HttpPost("sessions/{id:guid}/messages")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SendMessage(Guid id, [FromBody] SendCopilotMessageRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new SendCopilotMessageCommand(request.CompanyId, id, request.Message), cancellationToken);
        return Ok(result);
    }

    [HttpGet("sessions/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetCopilotSessionByIdQuery(companyId, id), cancellationToken);
        return Ok(result);
    }

    [HttpGet("sessions")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListForUser([FromQuery] Guid companyId, [FromQuery] Guid userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListCopilotSessionsForUserQuery(companyId, userId, page, pageSize), cancellationToken);
        return Ok(result);
    }
}

public sealed record StartCopilotSessionRequest(Guid CompanyId, Guid UserId, string? Title);
public sealed record SendCopilotMessageRequest(Guid CompanyId, string Message);
