using FusionOS.Modules.Hrms.Application.LeaveBalances.Commands.AdjustLeaveBalance;
using FusionOS.Modules.Hrms.Application.LeaveBalances.Commands.CreateLeaveBalance;
using FusionOS.Modules.Hrms.Application.LeaveBalances.Queries.GetLeaveBalance;
using FusionOS.Modules.Hrms.Application.LeaveBalances.Queries.ListLeaveBalancesForEmployee;
using FusionOS.Modules.Hrms.Domain.LeaveRequests;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Hrms.Api.Controllers;

/// <summary>
/// Phase 4 — HRMS: an employee's leave entitlement/accrual per LeaveType/Year,
/// wired into LeaveRequestsController's Approve action so approval can check
/// (and deduct from) a real balance instead of nothing at all.
/// </summary>
[ApiController]
[Route("api/v1/hrms/leave-balances")]
public sealed class LeaveBalancesController : ControllerBase
{
    private readonly ISender _sender;

    public LeaveBalancesController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateLeaveBalanceRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateLeaveBalanceCommand(request.CompanyId, request.EmployeeId, request.LeaveType, request.Year, request.EntitledDays);
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(Get), new { companyId = request.CompanyId, employeeId = request.EmployeeId, leaveType = request.LeaveType, year = request.Year }, result);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get([FromQuery] Guid companyId, [FromQuery] Guid employeeId, [FromQuery] LeaveType leaveType, [FromQuery] int year, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetLeaveBalanceQuery(companyId, employeeId, leaveType, year), cancellationToken);
        return Ok(result);
    }

    [HttpGet("by-employee/{employeeId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListForEmployee(Guid employeeId, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ListLeaveBalancesForEmployeeQuery(companyId, employeeId), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/adjust")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Adjust(Guid id, [FromBody] AdjustLeaveBalanceRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new AdjustLeaveBalanceCommand(request.CompanyId, id, request.NewEntitledDays), cancellationToken);
        return Ok(result);
    }
}

public sealed record CreateLeaveBalanceRequest(Guid CompanyId, Guid EmployeeId, LeaveType LeaveType, int Year, decimal EntitledDays);

public sealed record AdjustLeaveBalanceRequest(Guid CompanyId, decimal NewEntitledDays);
