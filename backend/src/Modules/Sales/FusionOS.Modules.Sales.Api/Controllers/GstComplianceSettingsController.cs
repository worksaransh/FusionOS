using FusionOS.Modules.Sales.Application.Settings.Commands.UpdateGstComplianceSettings;
using FusionOS.Modules.Sales.Application.Settings.Queries.GetGstComplianceSettings;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Sales.Api.Controllers;

/// <summary>
/// GST e-invoicing compliance toggle — gates GenerateEInvoiceCommandHandler. GET always
/// returns a row (GetGstComplianceSettingsQueryHandler creates defaults, EInvoicingRequired
/// = false, on first read), same "no separate bootstrap step" convention as Finance's
/// SettingsController.
/// </summary>
[ApiController]
[Route("api/v1/sales/gst-compliance-settings")]
public sealed class GstComplianceSettingsController : ControllerBase
{
    private readonly ISender _sender;

    public GstComplianceSettingsController(ISender sender) => _sender = sender;

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Get([FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetGstComplianceSettingsQuery(companyId), cancellationToken);
        return Ok(result);
    }

    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Update([FromBody] UpdateGstComplianceSettingsRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateGstComplianceSettingsCommand(request.CompanyId, request.EInvoicingRequired);
        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }
}

public sealed record UpdateGstComplianceSettingsRequest(Guid CompanyId, bool EInvoicingRequired);
