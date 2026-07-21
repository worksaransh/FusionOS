using FusionOS.Modules.Hrms.Application.Attendance.Commands.RecordAttendance;
using FusionOS.Modules.Hrms.Application.Attendance.Commands.UpdateAttendance;
using FusionOS.Modules.Hrms.Application.Attendance.Queries.GetAttendanceRecordById;
using FusionOS.Modules.Hrms.Application.Attendance.Queries.ListAttendanceRecords;
using FusionOS.Modules.Hrms.Domain.Attendance;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Hrms.Api.Controllers;

/// <summary>
/// Phase 4 — HRMS: an employee's attendance for a single calendar date.
/// Top-level (not nested under employees), same shape as LeaveRequestsController
/// — an employee-scoped resource filtered by an optional `employeeId` query
/// param, not a Warehouse-style nested route, so this stays consistent with
/// the rest of HRMS rather than that other module's convention.
/// </summary>
[ApiController]
[Route("api/v1/hrms/attendance-records")]
public sealed class AttendanceRecordsController : ControllerBase
{
    private readonly ISender _sender;

    public AttendanceRecordsController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] RecordAttendanceRequest request, CancellationToken cancellationToken)
    {
        var command = new RecordAttendanceCommand(
            request.CompanyId, request.EmployeeId, request.Date, request.CheckInTime, request.CheckOutTime, request.Status, request.LeaveRequestId);
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id, companyId = request.CompanyId }, result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetAttendanceRecordByIdQuery(companyId, id), cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        [FromQuery] Guid companyId,
        [FromQuery] Guid? employeeId = null,
        [FromQuery] DateTimeOffset? startDate = null,
        [FromQuery] DateTimeOffset? endDate = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListAttendanceRecordsQuery(companyId, employeeId, startDate, endDate, page, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAttendanceRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateAttendanceCommand(request.CompanyId, id, request.CheckInTime, request.CheckOutTime, request.Status, request.LeaveRequestId);
        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }
}

public sealed record RecordAttendanceRequest(
    Guid CompanyId,
    Guid EmployeeId,
    DateTimeOffset Date,
    DateTimeOffset? CheckInTime,
    DateTimeOffset? CheckOutTime,
    AttendanceStatus Status,
    Guid? LeaveRequestId);

public sealed record UpdateAttendanceRequest(
    Guid CompanyId,
    DateTimeOffset? CheckInTime,
    DateTimeOffset? CheckOutTime,
    AttendanceStatus Status,
    Guid? LeaveRequestId);
