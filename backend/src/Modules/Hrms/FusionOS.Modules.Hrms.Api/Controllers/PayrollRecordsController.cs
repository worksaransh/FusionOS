using FusionOS.Modules.Hrms.Application.Payroll.Commands.ApprovePayrollRecord;
using FusionOS.Modules.Hrms.Application.Payroll.Commands.CreatePayrollDraft;
using FusionOS.Modules.Hrms.Application.Payroll.Commands.MarkPayrollRecordPaid;
using FusionOS.Modules.Hrms.Application.Payroll.Queries.GetPayrollRecordById;
using FusionOS.Modules.Hrms.Application.Payroll.Queries.ListPayrollRecords;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Hrms.Api.Controllers;

/// <summary>
/// Phase 4 — HRMS: a deliberately minimal payroll skeleton, Draft -> Approved
/// -> Paid. See PayrollRecord.cs's own doc comment for exactly what this does
/// NOT compute (no allowances, no tax withholding, no statutory deductions).
/// Create-at-top-level shape, same as EmployeesController/
/// LeaveRequestsController; the two lifecycle transitions are dedicated POST
/// actions, same convention as LeaveRequestsController.Approve/Reject.
/// </summary>
[ApiController]
[Route("api/v1/hrms/payroll-records")]
public sealed class PayrollRecordsController : ControllerBase
{
    private readonly ISender _sender;

    public PayrollRecordsController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreatePayrollDraftRequest request, CancellationToken cancellationToken)
    {
        var command = new CreatePayrollDraftCommand(request.CompanyId, request.EmployeeId, request.PeriodMonth, request.PeriodYear, request.BaseSalary);
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id, companyId = request.CompanyId }, result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetPayrollRecordByIdQuery(companyId, id), cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        [FromQuery] Guid companyId,
        [FromQuery] Guid? employeeId = null,
        [FromQuery] int? periodMonth = null,
        [FromQuery] int? periodYear = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListPayrollRecordsQuery(companyId, employeeId, periodMonth, periodYear, page, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/approve")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Approve(Guid id, [FromBody] PayrollRecordActionRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ApprovePayrollRecordCommand(request.CompanyId, id), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/mark-paid")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkPaid(Guid id, [FromBody] PayrollRecordActionRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new MarkPayrollRecordPaidCommand(request.CompanyId, id), cancellationToken);
        return Ok(result);
    }
}

public sealed record CreatePayrollDraftRequest(Guid CompanyId, Guid EmployeeId, int PeriodMonth, int PeriodYear, decimal BaseSalary);

public sealed record PayrollRecordActionRequest(Guid CompanyId);
