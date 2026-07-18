using FusionOS.Modules.Hrms.Application.LeaveRequests.Commands.ApproveLeaveRequest;
using FusionOS.Modules.Hrms.Application.LeaveRequests.Commands.CreateLeaveRequest;
using FusionOS.Modules.Hrms.Application.LeaveRequests.Commands.RejectLeaveRequest;
using FusionOS.Modules.Hrms.Application.LeaveRequests.Queries.GetLeaveRequestById;
using FusionOS.Modules.Hrms.Application.LeaveRequests.Queries.ListLeaveRequests;
using FusionOS.Modules.Hrms.Domain.LeaveRequests;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Hrms.Api.Controllers;

/// <summary>
/// Phase 4 — HRMS: leave requests against an Employee, Requested → Approved/Rejected.
/// </summary>
[ApiController]
[Route("api/v1/hrms/leave-requests")]
public sealed class LeaveRequestsController : ControllerBase
{
    private readonly ISender _sender;

    public LeaveRequestsController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateLeaveRequestRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateLeaveRequestCommand(request.CompanyId, request.EmployeeId, request.Type, request.StartDate, request.EndDate, request.Reason);
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id, companyId = request.CompanyId }, result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetLeaveRequestByIdQuery(companyId, id), cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] Guid companyId, [FromQuery] Guid? employeeId = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListLeaveRequestsQuery(companyId, employeeId, page, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/approve")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Approve(Guid id, [FromBody] LeaveRequestActionRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ApproveLeaveRequestCommand(request.CompanyId, id), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/reject")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reject(Guid id, [FromBody] LeaveRequestActionRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new RejectLeaveRequestCommand(request.CompanyId, id), cancellationToken);
        return Ok(result);
    }
}

public sealed record CreateLeaveRequestRequest(Guid CompanyId, Guid EmployeeId, LeaveType Type, DateTimeOffset StartDate, DateTimeOffset EndDate, string? Reason);

public sealed record LeaveRequestActionRequest(Guid CompanyId);
