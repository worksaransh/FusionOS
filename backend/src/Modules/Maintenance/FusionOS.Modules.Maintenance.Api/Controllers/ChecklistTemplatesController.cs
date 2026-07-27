using FusionOS.Modules.Maintenance.Application.ChecklistTemplates.Commands.CreateChecklistTemplate;
using FusionOS.Modules.Maintenance.Application.ChecklistTemplates.Commands.DeactivateChecklistTemplate;
using FusionOS.Modules.Maintenance.Application.ChecklistTemplates.Queries.GetChecklistTemplateById;
using FusionOS.Modules.Maintenance.Application.ChecklistTemplates.Queries.ListChecklistTemplates;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Maintenance.Api.Controllers;

/// <summary>
/// Maintenance Checklists/Procedures (2026-07-23 follow-up): reusable named
/// procedures a MaintenanceRequest can optionally be created from — see
/// CreateMaintenanceRequestCommand's ChecklistTemplateId param. CRUD-ish shape
/// (create/read/list/soft-deactivate) mirroring AssetsController.
/// </summary>
[ApiController]
[Route("api/v1/maintenance/checklist-templates")]
public sealed class ChecklistTemplatesController : ControllerBase
{
    private readonly ISender _sender;

    public ChecklistTemplatesController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateChecklistTemplateRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateChecklistTemplateCommand(request.CompanyId, request.Name, request.Description, request.StepDescriptions);
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id, companyId = request.CompanyId }, result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetChecklistTemplateByIdQuery(companyId, id), cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] Guid companyId, [FromQuery] string? search = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListChecklistTemplatesQuery(companyId, search, page, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(Guid id, [FromBody] DeactivateChecklistTemplateRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeactivateChecklistTemplateCommand(request.CompanyId, id), cancellationToken);
        return Ok(result);
    }
}

public sealed record CreateChecklistTemplateRequest(Guid CompanyId, string Name, string? Description, IReadOnlyList<string> StepDescriptions);

public sealed record DeactivateChecklistTemplateRequest(Guid CompanyId);
