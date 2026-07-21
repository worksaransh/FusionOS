using FusionOS.Modules.Quality.Application.NonConformanceReports.Commands.CreateNonConformanceReport;
using FusionOS.Modules.Quality.Application.NonConformanceReports.Commands.UpdateNonConformanceReportStatus;
using FusionOS.Modules.Quality.Application.NonConformanceReports.Queries.GetNonConformanceReportById;
using FusionOS.Modules.Quality.Application.NonConformanceReports.Queries.ListNonConformanceReports;
using FusionOS.Modules.Quality.Domain.NonConformanceReports;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Quality.Api.Controllers;

/// <summary>
/// Phase 5 — Quality: Non-Conformance Reports (NCR). A defect or deviation, raised either
/// against a formal Inspection (InspectionId set) or standalone, Open -> UnderReview ->
/// Closed. CorrectiveActionsController's CAPA plans link back to one of these by id.
/// </summary>
[ApiController]
[Route("api/v1/quality/non-conformance-reports")]
public sealed class NonConformanceReportsController : ControllerBase
{
    private readonly ISender _sender;

    public NonConformanceReportsController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateNonConformanceReportRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateNonConformanceReportCommand(request.CompanyId, request.InspectionId, request.Description, request.Severity, request.RaisedByUserId);
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id, companyId = request.CompanyId }, result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetNonConformanceReportByIdQuery(companyId, id), cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] Guid companyId, [FromQuery] Guid? inspectionId = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListNonConformanceReportsQuery(companyId, inspectionId, page, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateNonConformanceReportStatusRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new UpdateNonConformanceReportStatusCommand(request.CompanyId, id, request.Status), cancellationToken);
        return Ok(result);
    }
}

public sealed record CreateNonConformanceReportRequest(Guid CompanyId, Guid? InspectionId, string Description, NonConformanceReportSeverity Severity, Guid RaisedByUserId);

public sealed record UpdateNonConformanceReportStatusRequest(Guid CompanyId, NonConformanceReportStatus Status);
