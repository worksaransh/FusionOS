using FusionOS.Modules.Finance.Application.WithholdingTaxRates.Commands.CreateWithholdingTaxRate;
using FusionOS.Modules.Finance.Application.WithholdingTaxRates.Commands.DeactivateWithholdingTaxRate;
using FusionOS.Modules.Finance.Application.WithholdingTaxRates.Queries.ListWithholdingTaxRates;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Finance.Api.Controllers;

/// <summary>
/// TDS/TCS rates under the Income Tax Act (e.g. section 194C, 194J, 206C(1H)) — see
/// WithholdingTaxRate's class doc comment for why this is modeled separately from GST's
/// TaxRate/TaxJurisdiction.
/// </summary>
[ApiController]
[Route("api/v1/finance/withholding-tax-rates")]
public sealed class WithholdingTaxRatesController : ControllerBase
{
    private readonly ISender _sender;

    public WithholdingTaxRatesController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateWithholdingTaxRateRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateWithholdingTaxRateCommand(request.CompanyId, request.SectionCode, request.Description, request.Percentage);
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(List), new { companyId = request.CompanyId }, result);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] Guid companyId, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListWithholdingTaxRatesQuery(companyId, page, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(Guid id, [FromBody] DeactivateWithholdingTaxRateRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeactivateWithholdingTaxRateCommand(request.CompanyId, id), cancellationToken);
        return Ok(result);
    }
}

public sealed record CreateWithholdingTaxRateRequest(Guid CompanyId, string SectionCode, string Description, decimal Percentage);

public sealed record DeactivateWithholdingTaxRateRequest(Guid CompanyId);
