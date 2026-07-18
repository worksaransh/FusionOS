using FusionOS.Modules.Finance.Application.TaxRates.Commands.CreateTaxRate;
using FusionOS.Modules.Finance.Application.TaxRates.Commands.DeactivateTaxRate;
using FusionOS.Modules.Finance.Application.TaxRates.Commands.UpdateTaxRate;
using FusionOS.Modules.Finance.Application.TaxRates.Queries.CalculateLineTax;
using FusionOS.Modules.Finance.Application.TaxRates.Queries.GetTaxRateById;
using FusionOS.Modules.Finance.Application.TaxRates.Queries.ListTaxRates;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Finance.Api.Controllers;

/// <summary>
/// M8b — Finance depth: multi-jurisdiction tax engine. A TaxRate nests under
/// a TaxJurisdiction via TaxJurisdictionId (query-string/body scoped, not
/// path-nested — TaxJurisdiction is the picked parent much like Account is
/// for JournalEntry lines, rather than a path-nested resource like
/// Bin under Zone/Warehouse).
/// </summary>
[ApiController]
[Route("api/v1/finance/tax-rates")]
public sealed class TaxRatesController : ControllerBase
{
    private readonly ISender _sender;

    public TaxRatesController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateTaxRateRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateTaxRateCommand(request.CompanyId, request.TaxJurisdictionId, request.Code, request.Name, request.Percentage);
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id, companyId = request.CompanyId }, result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetTaxRateByIdQuery(companyId, id), cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] Guid companyId, [FromQuery] Guid taxJurisdictionId, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListTaxRatesQuery(companyId, taxJurisdictionId, page, pageSize), cancellationToken);
        return Ok(result);
    }

    // Read-only utility endpoint, not a resource under this collection — kept on this
    // controller since it's purely a query over TaxRate data, the same "utility action
    // alongside CRUD" placement as ExchangeRatesController.Convert.
    [HttpGet("calculate-line-tax")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CalculateLineTax([FromQuery] Guid companyId, [FromQuery] Guid taxRateId, [FromQuery] decimal netAmount, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new CalculateLineTaxQuery(companyId, taxRateId, netAmount), cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTaxRateRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateTaxRateCommand(request.CompanyId, id, request.Name, request.Percentage);
        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }

    // Soft-deactivate only — never a real delete (04_DATABASE_GUIDELINES.md), same
    // body-bound-request convention as CostCentersController.Deactivate.
    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(Guid id, [FromBody] DeactivateTaxRateRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeactivateTaxRateCommand(request.CompanyId, id), cancellationToken);
        return Ok(result);
    }
}

public sealed record CreateTaxRateRequest(Guid CompanyId, Guid TaxJurisdictionId, string Code, string Name, decimal Percentage);

public sealed record UpdateTaxRateRequest(Guid CompanyId, string Name, decimal Percentage);

public sealed record DeactivateTaxRateRequest(Guid CompanyId);
