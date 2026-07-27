using FusionOS.Modules.Quality.Application.Calibrations.Commands.CreateCalibrationRecord;
using FusionOS.Modules.Quality.Application.Calibrations.Commands.RecordCalibrationEvent;
using FusionOS.Modules.Quality.Application.Calibrations.Queries.GetCalibrationRecordById;
using FusionOS.Modules.Quality.Application.Calibrations.Queries.ListCalibrationRecords;
using FusionOS.Modules.Quality.Domain.Calibrations;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Quality.Api.Controllers;

/// <summary>
/// Phase 5 — Quality: calibration tracking for measurement/test equipment. One row per piece
/// of equipment holding its current calibration state (see CalibrationRecord's doc comment
/// for the current-state-vs-history scope decision); RecordCalibration overwrites it on every
/// new calibration event.
/// </summary>
[ApiController]
[Route("api/v1/quality/calibrations")]
public sealed class CalibrationRecordsController : ControllerBase
{
    private readonly ISender _sender;

    public CalibrationRecordsController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateCalibrationRecordRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateCalibrationRecordCommand(
            request.CompanyId,
            request.EquipmentReferenceId,
            request.EquipmentName,
            request.EquipmentIdentifier,
            request.CalibrationStandard,
            request.LastCalibratedDate,
            request.NextCalibrationDueDate,
            request.Result,
            request.PerformedByUserId,
            request.PerformedByExternalVendor,
            request.Notes);
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id, companyId = request.CompanyId }, result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetCalibrationRecordByIdQuery(companyId, id), cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        [FromQuery] Guid companyId, [FromQuery] bool overdueOnly = false, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListCalibrationRecordsQuery(companyId, overdueOnly, page, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/calibrate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RecordCalibration(Guid id, [FromBody] RecordCalibrationEventRequest request, CancellationToken cancellationToken)
    {
        var command = new RecordCalibrationEventCommand(
            request.CompanyId, id, request.CalibratedDate, request.NextDueDate, request.Result, request.PerformedByUserId, request.PerformedByExternalVendor, request.Notes);
        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }
}

public sealed record CreateCalibrationRecordRequest(
    Guid CompanyId,
    Guid EquipmentReferenceId,
    string EquipmentName,
    string? EquipmentIdentifier,
    string? CalibrationStandard,
    DateTimeOffset LastCalibratedDate,
    DateTimeOffset NextCalibrationDueDate,
    CalibrationResult Result,
    Guid? PerformedByUserId,
    string? PerformedByExternalVendor,
    string? Notes);

public sealed record RecordCalibrationEventRequest(
    Guid CompanyId,
    DateTimeOffset CalibratedDate,
    DateTimeOffset NextDueDate,
    CalibrationResult Result,
    Guid? PerformedByUserId,
    string? PerformedByExternalVendor,
    string? Notes);
