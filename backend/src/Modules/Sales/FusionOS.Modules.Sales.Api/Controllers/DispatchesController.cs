using FusionOS.Modules.Sales.Application.Dispatches.Commands.CreateDispatch;
using FusionOS.Modules.Sales.Application.Dispatches.Queries.ListDispatches;
using FusionOS.Modules.Sales.Domain.Dispatches;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Sales.Api.Controllers;

/// <summary>Dispatches — next slice after Sales Orders (05_MODULE_ROADMAP.md Phase 1: Sales capability list — "Dispatch").</summary>
[ApiController]
[Route("api/v1/sales/dispatches")]
public sealed class DispatchesController : ControllerBase
{
    private readonly ISender _sender;

    public DispatchesController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateDispatchRequest request, CancellationToken cancellationToken)
    {
        var lines = request.Lines.Select(l => new DispatchLineInput(l.ProductId, l.QuantityDispatched)).ToList();
        var command = new CreateDispatchCommand(request.CompanyId, request.SalesOrderId, request.WarehouseId, lines);
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(List), new { companyId = request.CompanyId }, result);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] Guid companyId, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListDispatchesQuery(companyId, page, pageSize), cancellationToken);
        return Ok(result);
    }
}

public sealed record CreateDispatchLineRequest(Guid ProductId, decimal QuantityDispatched);

public sealed record CreateDispatchRequest(Guid CompanyId, Guid SalesOrderId, Guid WarehouseId, IReadOnlyList<CreateDispatchLineRequest> Lines);
