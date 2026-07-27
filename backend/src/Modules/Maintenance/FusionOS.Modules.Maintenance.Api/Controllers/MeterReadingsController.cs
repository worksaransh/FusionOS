using FusionOS.Modules.Maintenance.Application.MeterReadings.Commands.RecordMeterReading;
using FusionOS.Modules.Maintenance.Application.MeterReadings.Queries.GetLatestMeterReadingForAsset;
using FusionOS.Modules.Maintenance.Application.MeterReadings.Queries.ListMeterReadingsForAsset;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Maintenance.Api.Controllers;

/// <summary>
/// Meter/usage reading capture against an Asset — hours run, cycles, mileage,
/// whatever unit that machine's meter reports. Independent of
/// MaintenanceSchedule's calendar-based due-date trigger; see
/// MaintenanceScheduleFrequency.cs's doc comment for why usage-based scheduling
/// itself stays out of scope here. This controller only records and lists
/// readings.
/// </summary>
[ApiController]
[Route("api/v1/maintenance/meter-readings")]
public sealed class MeterReadingsController : ControllerBase
{
    private readonly ISender _sender;

    public MeterReadingsController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Record([FromBody] RecordMeterReadingRequest request, CancellationToken cancellationToken)
    {
        var command = new RecordMeterReadingCommand(request.CompanyId, request.AssetId, request.ReadingValue, request.ReadingDate, request.Unit);
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(ListForAsset), new { companyId = request.CompanyId, assetId = request.AssetId }, result);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ListForAsset([FromQuery] Guid companyId, [FromQuery] Guid assetId, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListMeterReadingsForAssetQuery(companyId, assetId, page, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpGet("latest")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLatestForAsset([FromQuery] Guid companyId, [FromQuery] Guid assetId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetLatestMeterReadingForAssetQuery(companyId, assetId), cancellationToken);
        return Ok(result);
    }
}

public sealed record RecordMeterReadingRequest(Guid CompanyId, Guid AssetId, decimal ReadingValue, DateTimeOffset ReadingDate, string Unit);
