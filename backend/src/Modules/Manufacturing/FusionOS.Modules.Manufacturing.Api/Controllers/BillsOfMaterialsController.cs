using FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Commands.AddRoutingOperation;
using FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Commands.CreateBillOfMaterials;
using FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Commands.CreateNewBomVersion;
using FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Commands.DeactivateBillOfMaterials;
using FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Commands.RemoveRoutingOperation;
using FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Commands.ReorderRoutingOperations;
using FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Commands.SetPrimaryBillOfMaterials;
using FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Commands.UpdateBillOfMaterials;
using FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Contracts;
using FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Queries.ExplodeBillOfMaterials;
using FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Queries.GetBillOfMaterialsById;
using FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Queries.GetVersionHistory;
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

    /// <summary>Header-level-only update (Name, ProductId) — deliberately distinct from CreateNewVersion/SetPrimary; does not touch Lines/Operations or bump Version.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBillOfMaterialsRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateBillOfMaterialsCommand(request.CompanyId, id, request.Name, request.ProductId);
        var result = await _sender.Send(command, cancellationToken);
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

    /// <summary>Appends a routing operation (e.g. "Cut", "Assemble", "Paint") to this bill of materials' production routing.</summary>
    [HttpPost("{id:guid}/operations")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddOperation(Guid id, [FromBody] AddRoutingOperationRequest request, CancellationToken cancellationToken)
    {
        var command = new AddRoutingOperationCommand(request.CompanyId, id, request.OperationName, request.WorkCenter, request.StandardMinutes);
        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>Reassigns the whole routing's sequence order — the body must list every existing operation id exactly once, in the new order.</summary>
    [HttpPost("{id:guid}/operations/reorder")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReorderOperations(Guid id, [FromBody] ReorderRoutingOperationsRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ReorderRoutingOperationsCommand(request.CompanyId, id, request.OrderedOperationIds), cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}/operations/{operationId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveOperation(Guid id, Guid operationId, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new RemoveRoutingOperationCommand(companyId, id, operationId), cancellationToken);
        return Ok(result);
    }

    /// <summary>BOM Versioning (Phase 6): supersedes this bill of materials with a new Version + 1 row, copying its routing operations.</summary>
    [HttpPost("{id:guid}/new-version")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateNewVersion(Guid id, [FromBody] CreateNewBomVersionRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateNewBomVersionCommand(request.CompanyId, id, request.Name, request.Lines);
        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>Alternative BOM (Phase 6): marks this bill of materials as the primary one for its product, clearing the flag on every other active BOM for that product.</summary>
    [HttpPost("{id:guid}/set-primary")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetPrimary(Guid id, [FromBody] SetPrimaryBillOfMaterialsRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new SetPrimaryBillOfMaterialsCommand(request.CompanyId, id), cancellationToken);
        return Ok(result);
    }

    /// <summary>BOM Versioning (Phase 6): every version in this bill of materials' version chain (including superseded ones), newest first.</summary>
    [HttpGet("{id:guid}/version-history")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetVersionHistory(Guid id, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetVersionHistoryQuery(companyId, id), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Multi-Level BOM explosion (Phase 6): flattens this bill of materials, recursing into
    /// any component's own primary BillOfMaterials, down to its leaf (non-manufactured)
    /// component requirements for the given quantity. POST rather than GET because the
    /// input (a quantity) doesn't fit this resource's query-string shape any more
    /// awkwardly than a body would — same pragmatic choice as other read-only-but-bodied
    /// actions in this codebase; it does not create or mutate anything.
    /// </summary>
    [HttpPost("{id:guid}/explode")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Explode(Guid id, [FromBody] ExplodeBillOfMaterialsRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ExplodeBillOfMaterialsQuery(request.CompanyId, id, request.Quantity), cancellationToken);
        return Ok(result);
    }
}

public sealed record CreateBillOfMaterialsRequest(Guid CompanyId, string Code, string Name, Guid ProductId, IReadOnlyList<BomLineInput> Lines);

public sealed record UpdateBillOfMaterialsRequest(Guid CompanyId, string Name, Guid ProductId);

public sealed record DeactivateBillOfMaterialsRequest(Guid CompanyId);

public sealed record AddRoutingOperationRequest(Guid CompanyId, string OperationName, string WorkCenter, decimal StandardMinutes);

public sealed record ReorderRoutingOperationsRequest(Guid CompanyId, IReadOnlyList<Guid> OrderedOperationIds);

public sealed record CreateNewBomVersionRequest(Guid CompanyId, string Name, IReadOnlyList<BomLineInput> Lines);

public sealed record SetPrimaryBillOfMaterialsRequest(Guid CompanyId);

public sealed record ExplodeBillOfMaterialsRequest(Guid CompanyId, decimal Quantity);
