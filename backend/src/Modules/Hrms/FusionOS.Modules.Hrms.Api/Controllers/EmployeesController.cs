using FusionOS.Modules.Hrms.Application.Employees.Commands.CreateEmployee;
using FusionOS.Modules.Hrms.Application.Employees.Commands.DeactivateEmployee;
using FusionOS.Modules.Hrms.Application.Employees.Commands.UpdateEmployee;
using FusionOS.Modules.Hrms.Application.Employees.Queries.GetEmployeeById;
using FusionOS.Modules.Hrms.Application.Employees.Queries.GetEmployeeOrgChart;
using FusionOS.Modules.Hrms.Application.Employees.Queries.ListEmployees;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Hrms.Api.Controllers;

/// <summary>
/// Phase 4 — HRMS: employee records. CRUD-ish shape (create/read/list/soft-deactivate)
/// mirroring CostCentersController/AssetsController.
/// </summary>
[ApiController]
[Route("api/v1/hrms/employees")]
public sealed class EmployeesController : ControllerBase
{
    private readonly ISender _sender;

    public EmployeesController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateEmployeeRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateEmployeeCommand(request.CompanyId, request.Code, request.FullName, request.Email, request.DepartmentName, request.HireDate, request.DepartmentId, request.ManagerId);
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id, companyId = request.CompanyId }, result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEmployeeRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateEmployeeCommand(request.CompanyId, id, request.FullName, request.Email, request.DepartmentId, request.ManagerId);
        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}/org-chart")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrgChart(Guid id, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetEmployeeOrgChartQuery(companyId, id), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetEmployeeByIdQuery(companyId, id), cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] Guid companyId, [FromQuery] string? search = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListEmployeesQuery(companyId, search, page, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(Guid id, [FromBody] DeactivateEmployeeRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeactivateEmployeeCommand(request.CompanyId, id), cancellationToken);
        return Ok(result);
    }
}

public sealed record CreateEmployeeRequest(Guid CompanyId, string Code, string FullName, string Email, string? DepartmentName, DateTimeOffset HireDate, Guid? DepartmentId = null, Guid? ManagerId = null);

public sealed record UpdateEmployeeRequest(Guid CompanyId, string FullName, string Email, Guid? DepartmentId, Guid? ManagerId);

public sealed record DeactivateEmployeeRequest(Guid CompanyId);
