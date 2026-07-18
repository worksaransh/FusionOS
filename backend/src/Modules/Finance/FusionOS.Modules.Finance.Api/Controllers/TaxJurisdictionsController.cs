using FusionOS.Modules.Finance.Application.TaxJurisdictions.Commands.CreateTaxJurisdiction;
using FusionOS.Modules.Finance.Application.TaxJurisdictions.Commands.DeactivateTaxJurisdiction;
using FusionOS.Modules.Finance.Application.TaxJurisdictions.Commands.UpdateTaxJurisdiction;
using FusionOS.Modules.Finance.Application.TaxJurisdictions.Queries.GetTaxJurisdictionById;
using FusionOS.Modules.Finance.Application.TaxJurisdictions.Queries.ListTaxJurisdictions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Finance.Api.Controllers;

/// <summary>M8b — Finance depth: multi-jurisdiction tax engine. Pure master data, same shape as CostCentersController.</summary>
[ApiController]
[Route("api/v1/finance/tax-jurisdictions")]
public sealed class TaxJurisdictionsController : ControllerBase
{
    private readonly ISender _sender;

    public TaxJurisdictionsController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateTaxJurisdictionRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateTaxJurisdictionCommand(request.CompanyId, request.Code, request.Name);
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id, companyId = request.CompanyId }, result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetTaxJurisdictionByIdQuery(companyId, id), cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] Guid companyId, [FromQuery] string? search = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListTaxJurisdictionsQuery(companyId, search, page, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTaxJurisdictionRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateTaxJurisdictionCommand(request.CompanyId, id, request.Name);
        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }

    // Soft-deactivate only — never a real delete (04_DATABASE_GUIDELINES.md), same
    // body-bound-request convention as CostCentersController.Deactivate.
    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(Guid id, [FromBody] DeactivateTaxJurisdictionRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeactivateTaxJurisdictionCommand(request.CompanyId, id), cancellationToken);
        return Ok(result);
    }
}

public sealed record CreateTaxJurisdictionRequest(Guid CompanyId, string Code, string Name);

public sealed record UpdateTaxJurisdictionRequest(Guid CompanyId, string Name);

public sealed record DeactivateTaxJurisdictionRequest(Guid CompanyId);
