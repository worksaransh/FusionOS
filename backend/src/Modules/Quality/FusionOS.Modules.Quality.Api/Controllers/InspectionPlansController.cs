using FusionOS.Modules.Quality.Application.InspectionPlans.Commands.CreateInspectionPlan;
using FusionOS.Modules.Quality.Application.InspectionPlans.Commands.DeactivateInspectionPlan;
using FusionOS.Modules.Quality.Application.InspectionPlans.Queries.GetInspectionPlanById;
using FusionOS.Modules.Quality.Application.InspectionPlans.Queries.ListInspectionPlans;
using FusionOS.Modules.Quality.Domain.Inspections;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Quality.Api.Controllers;

/// <summary>
/// Phase 5 — Quality: Inspection Plans, a reusable checklist template for a given
/// InspectionType. Purely a convenience default for InspectionsController's Create — see
/// CreateInspectionCommand's InspectionPlanId doc comment.
/// </summary>
[ApiController]
[Route("api/v1/quality/inspection-plans")]
public sealed class InspectionPlansController : ControllerBase
{
    private readonly ISender _sender;

    public InspectionPlansController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateInspectionPlanRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateInspectionPlanCommand(request.CompanyId, request.Name, request.InspectionType, request.CharacteristicNames);
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id, companyId = request.CompanyId }, result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetInspectionPlanByIdQuery(companyId, id), cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] Guid companyId, [FromQuery] InspectionType? inspectionType = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListInspectionPlansQuery(companyId, inspectionType, page, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(Guid id, [FromBody] DeactivateInspectionPlanRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeactivateInspectionPlanCommand(request.CompanyId, id), cancellationToken);
        return Ok(result);
    }
}

public sealed record CreateInspectionPlanRequest(Guid CompanyId, string Name, InspectionType InspectionType, IReadOnlyList<string> CharacteristicNames);

public sealed record DeactivateInspectionPlanRequest(Guid CompanyId);
