using FusionOS.Modules.Quality.Application.Inspections.Commands.CreateInspection;
using FusionOS.Modules.Quality.Application.Inspections.Commands.RecordInspectionResults;
using FusionOS.Modules.Quality.Application.Inspections.Queries.GetInspectionById;
using FusionOS.Modules.Quality.Application.Inspections.Queries.ListInspections;
using FusionOS.Modules.Quality.Domain.Inspections;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Quality.Api.Controllers;

/// <summary>
/// Phase 5 — Quality: Inspections. A checklist inspection of a Manufacturing Work Order's
/// output (Production) or a Goods Receipt (IncomingGoods), created Pending with a set of
/// characteristics, then resolved to Passed/Failed once each result is recorded.
/// </summary>
[ApiController]
[Route("api/v1/quality/inspections")]
public sealed class InspectionsController : ControllerBase
{
    private readonly ISender _sender;

    public InspectionsController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateInspectionRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateInspectionCommand(request.CompanyId, request.Type, request.ReferenceId, request.Characteristics);
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id, companyId = request.CompanyId }, result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetInspectionByIdQuery(companyId, id), cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] Guid companyId, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListInspectionsQuery(companyId, page, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/results")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RecordResults(Guid id, [FromBody] RecordInspectionResultsRequest request, CancellationToken cancellationToken)
    {
        var command = new RecordInspectionResultsCommand(request.CompanyId, id, request.Results);
        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }
}

public sealed record CreateInspectionRequest(Guid CompanyId, InspectionType Type, Guid ReferenceId, IReadOnlyList<string> Characteristics);

public sealed record RecordInspectionResultsRequest(Guid CompanyId, IReadOnlyList<InspectionResultInput> Results);
