using FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Commands.CreateBillOfMaterials;
using FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Commands.DeactivateBillOfMaterials;
using FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Queries.GetBillOfMaterialsById;
using FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Queries.ListBillOfMaterials;
using FusionOS.Modules.Manufacturing.Domain.BillOfMaterials;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Manufacturing.Api.Controllers;

/// <summary>
/// Phase 3 — Manufacturing ERP: Bills of Materials. Master data defining what a
/// manufactured product is made of. CRUD-ish shape (create/read/list/soft-deactivate)
/// mirroring CostCentersController; components are supplied inline on the create body.
/// </summary>
[ApiController]
[Route("api/v1/manufacturing/bills-of-materials")]
public sealed class BillsOfMaterialsController : ControllerBase
{
    private readonly ISender _sender;

    public BillsOfMaterialsController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateBillOfMaterialsRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateBillOfMaterialsCommand(request.CompanyId, request.Code, request.Name, request.ProductId, request.Lines);
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id, companyId = request.CompanyId }, result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetBillOfMaterialsByIdQuery(companyId, id), cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] Guid companyId, [FromQuery] string? search = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListBillOfMaterialsQuery(companyId, search, page, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(Guid id, [FromBody] DeactivateBillOfMaterialsRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeactivateBillOfMaterialsCommand(request.CompanyId, id), cancellationToken);
        return Ok(result);
    }
}

public sealed record CreateBillOfMaterialsRequest(Guid CompanyId, string Code, string Name, Guid ProductId, IReadOnlyList<BomLineInput> Lines);

public sealed record DeactivateBillOfMaterialsRequest(Guid CompanyId);
