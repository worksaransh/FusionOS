using FusionOS.Modules.Core.Application.Departments.Commands.CreateDepartment;
using FusionOS.Modules.Core.Application.Departments.Commands.DeactivateDepartment;
using FusionOS.Modules.Core.Application.Departments.Commands.UpdateDepartment;
using FusionOS.Modules.Core.Application.Departments.Queries.GetDepartmentById;
using FusionOS.Modules.Core.Application.Departments.Queries.ListDepartments;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Core.Api.Controllers;

/// <summary>Departments under a Company (optionally scoped to a Branch, optionally nested under a parent Department) — pure master data, same shape as BranchesController.</summary>
[ApiController]
[Route("api/v1/core/departments")]
public sealed class DepartmentsController : ControllerBase
{
    private readonly ISender _sender;

    public DepartmentsController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateDepartmentRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateDepartmentCommand(request.CompanyId, request.BranchId, request.Name, request.Code, request.ParentDepartmentId);
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id, companyId = request.CompanyId }, result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetDepartmentByIdQuery(companyId, id), cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] Guid companyId, [FromQuery] string? search = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListDepartmentsQuery(companyId, search, page, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDepartmentRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateDepartmentCommand(request.CompanyId, id, request.Name, request.BranchId, request.ParentDepartmentId);
        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }

    // Soft-deactivate only — never a real delete, same convention as BranchesController.
    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(Guid id, [FromBody] DeactivateDepartmentRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeactivateDepartmentCommand(request.CompanyId, id), cancellationToken);
        return Ok(result);
    }
}

public sealed record CreateDepartmentRequest(Guid CompanyId, Guid? BranchId, string Name, string Code, Guid? ParentDepartmentId = null);

public sealed record UpdateDepartmentRequest(Guid CompanyId, string Name, Guid? BranchId, Guid? ParentDepartmentId);

public sealed record DeactivateDepartmentRequest(Guid CompanyId);
