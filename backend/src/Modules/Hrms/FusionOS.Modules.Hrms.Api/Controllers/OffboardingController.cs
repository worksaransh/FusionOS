using FusionOS.Modules.Hrms.Application.Offboarding.Commands.CompleteOffboardingTask;
using FusionOS.Modules.Hrms.Application.Offboarding.Commands.CreateOffboardingWorkflow;
using FusionOS.Modules.Hrms.Application.Offboarding.Queries.GetOffboardingWorkflow;
using FusionOS.Modules.Hrms.Application.Offboarding.Queries.ListOffboardingWorkflows;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Hrms.Api.Controllers;

/// <summary>
/// Offboarding checklists for departing employees. Backed by the same EmployeeWorkflow entity as
/// OnboardingController (see EmployeeWorkflow.cs's own doc comment for why) — this controller only
/// ever sees/creates WorkflowKind.Offboarding instances, enforced by the handlers underneath.
/// Starting or completing an offboarding workflow never deactivates the employee itself — that
/// remains a separate call to POST .../hrms/employees/{id}/deactivate (see
/// CreateOffboardingWorkflowCommand's own doc comment).
/// </summary>
[ApiController]
[Route("api/v1/hrms/offboarding")]
public sealed class OffboardingController : ControllerBase
{
    private readonly ISender _sender;

    public OffboardingController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateOffboardingWorkflowRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateOffboardingWorkflowCommand(request.CompanyId, request.EmployeeId, request.TaskTitles);
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id, companyId = request.CompanyId }, result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetOffboardingWorkflowQuery(companyId, id), cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] Guid companyId, [FromQuery] Guid? employeeId = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListOffboardingWorkflowsQuery(companyId, employeeId, page, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/tasks/{taskId:guid}/complete")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CompleteTask(Guid id, Guid taskId, [FromBody] CompleteWorkflowTaskRequest request, CancellationToken cancellationToken)
    {
        var command = new CompleteOffboardingTaskCommand(request.CompanyId, id, taskId, request.Notes);
        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }
}

public sealed record CreateOffboardingWorkflowRequest(Guid CompanyId, Guid EmployeeId, IReadOnlyCollection<string> TaskTitles);
