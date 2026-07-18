using FusionOS.Modules.Procurement.Application.SupplierContracts.Commands.CreateSupplierContract;
using FusionOS.Modules.Procurement.Application.SupplierContracts.Commands.TerminateSupplierContract;
using FusionOS.Modules.Procurement.Application.SupplierContracts.Queries.ListSupplierContracts;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Procurement.Api.Controllers;

/// <summary>Supplier contracts — validity period + terms, alongside supplier scorecards as the two remaining Procurement-depth items (docs/IMPLEMENTATION_PLAN.md Phase 10 item 2). Terminate is modeled as a sub-resource action per 08_API_STANDARDS.md §3, same as Award/Convert on RfqsController.</summary>
[ApiController]
[Route("api/v1/procurement/supplier-contracts")]
public sealed class SupplierContractsController : ControllerBase
{
    private readonly ISender _sender;

    public SupplierContractsController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateSupplierContractCommand command, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(List), new { companyId = command.CompanyId }, result);
    }

    [HttpPost("{id:guid}/terminate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Terminate(Guid id, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new TerminateSupplierContractCommand(companyId, id), cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] Guid companyId, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListSupplierContractsQuery(companyId, page, pageSize), cancellationToken);
        return Ok(result);
    }
}
