using FusionOS.Modules.Hrms.Application.Onboarding.Commands.CompleteOnboardingTask;
using FusionOS.Modules.Hrms.Application.Onboarding.Commands.CreateOnboardingWorkflow;
using FusionOS.Modules.Hrms.Application.Onboarding.Queries.GetOnboardingWorkflow;
using FusionOS.Modules.Hrms.Application.Onboarding.Queries.ListOnboardingWorkflows;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Hrms.Api.Controllers;

/// <summary>
/// Onboarding checklists for new employees. Backed by the same EmployeeWorkflow entity as
/// OffboardingController (see EmployeeWorkflow.cs's own doc comment for why) — this controller
/// only ever sees/creates WorkflowKind.Onboarding instances, enforced by the handlers underneath.
/// </summary>
[ApiController]
[Route("api/v1/hrms/onboarding")]
public sealed class OnboardingController : ControllerBase
{
    private readonly ISender _sender;

    public OnboardingController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateOnboardingWorkflowRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateOnboardingWorkflowCommand(request.CompanyId, request.EmployeeId, request.TaskTitles);
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id, companyId = request.CompanyId }, result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetOnboardingWorkflowQuery(companyId, id), cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] Guid companyId, [FromQuery] Guid? employeeId = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListOnboardingWorkflowsQuery(companyId, employeeId, page, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/tasks/{taskId:guid}/complete")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CompleteTask(Guid id, Guid taskId, [FromBody] CompleteWorkflowTaskRequest request, CancellationToken cancellationToken)
    {
        var command = new CompleteOnboardingTaskCommand(request.CompanyId, id, taskId, request.Notes);
        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }
}

public sealed record CreateOnboardingWorkflowRequest(Guid CompanyId, Guid EmployeeId, IReadOnlyCollection<string> TaskTitles);

public sealed record CompleteWorkflowTaskRequest(Guid CompanyId, string? Notes = null);
