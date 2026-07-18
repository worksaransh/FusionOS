using FusionOS.Modules.Core.Application.Workflow.Commands.CreateApprovalRequest;
using FusionOS.Modules.Core.Application.Workflow.Commands.DecideApprovalStep;
using FusionOS.Modules.Core.Application.Workflow.Queries.GetApprovalRequest;
using FusionOS.Modules.Core.Application.Workflow.Queries.ListPendingApprovals;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Core.Api.Controllers;

/// <summary>
/// Generic multi-step approval engine (Phase M7, 2026-07-15) — see
/// FusionOS.Modules.Core.Domain.Workflow.ApprovalRequest's doc comment for
/// why this is a standalone, opt-in capability rather than a retrofit of
/// existing per-module Approve() actions like Procurement's PurchaseOrder.
/// </summary>
[ApiController]
[Route("api/v1/core/approvals")]
public sealed class ApprovalsController : ControllerBase
{
    private readonly ISender _sender;

    public ApprovalsController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateApprovalRequestCommand command, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id, companyId = command.CompanyId }, result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetApprovalRequestQuery(companyId, id), cancellationToken);
        return Ok(result);
    }

    [HttpGet("pending-for-me")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ListPendingForMe([FromQuery] Guid companyId, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListPendingApprovalsQuery(companyId, page, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/decide")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Decide(Guid id, [FromBody] DecideApprovalStepRequest request, CancellationToken cancellationToken)
    {
        var command = new DecideApprovalStepCommand(request.CompanyId, id, request.Approve, request.Comments);
        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }
}

public sealed record DecideApprovalStepRequest(Guid CompanyId, bool Approve, string? Comments);
