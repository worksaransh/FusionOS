using FusionOS.Modules.Sales.Application.PriceLists.Commands.CreatePriceList;
using FusionOS.Modules.Sales.Application.PriceLists.Queries.ListPriceLists;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Sales.Api.Controllers;

/// <summary>Price lists — the "multiple price lists per customer segment" half of the pricing/discount engine (docs/IMPLEMENTATION_PLAN.md Phase 10 item 10). Assignment to a Customer is a sub-resource action on CustomersController.</summary>
[ApiController]
[Route("api/v1/sales/price-lists")]
public sealed class PriceListsController : ControllerBase
{
    private readonly ISender _sender;

    public PriceListsController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreatePriceListCommand command, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(List), new { companyId = command.CompanyId }, result);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] Guid companyId, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListPriceListsQuery(companyId, page, pageSize), cancellationToken);
        return Ok(result);
    }
}
