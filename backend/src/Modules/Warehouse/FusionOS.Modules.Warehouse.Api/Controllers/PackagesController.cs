using FusionOS.Modules.Warehouse.Application.Packages.Commands.CreatePackage;
using FusionOS.Modules.Warehouse.Application.Packages.Queries.GetPackageById;
using FusionOS.Modules.Warehouse.Application.Packages.Queries.ListPackagesByPickList;
using FusionOS.Modules.Warehouse.Domain.Packages;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Warehouse.Api.Controllers;

/// <summary>
/// Packages (2026-07-21 packing depth pass) — nested under pick-lists exactly like
/// PickListsController nests under warehouses, since a Package only ever exists in the context of
/// one already-Packed PickList. Records WHICH items went into WHICH physical carton, plus optional
/// weight/dimensions — detail PickList.Pack()'s status flag alone never captured. Purely additive:
/// does not change or replace PickList.Pack()'s existing behavior in any way.
/// </summary>
[ApiController]
[Route("api/v1/warehouse/warehouses/{warehouseId:guid}/pick-lists/{pickListId:guid}/packages")]
public sealed class PackagesController : ControllerBase
{
    private readonly ISender _sender;

    public PackagesController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create(Guid warehouseId, Guid pickListId, [FromBody] CreatePackageRequest request, CancellationToken cancellationToken)
    {
        var lines = request.Lines.Select(l => new PackageLineInput(l.ProductId, l.Quantity)).ToList();
        var command = new CreatePackageCommand(
            request.CompanyId,
            pickListId,
            request.PackageNumber,
            request.WeightKg,
            request.LengthCm,
            request.WidthCm,
            request.HeightCm,
            lines);
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { warehouseId, pickListId, id = result.Id, companyId = request.CompanyId }, result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid warehouseId, Guid pickListId, Guid id, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetPackageByIdQuery(companyId, id), cancellationToken);
        if (result is null || result.PickListId != pickListId)
            return NotFound();
        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List(Guid warehouseId, Guid pickListId, [FromQuery] Guid companyId, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListPackagesByPickListQuery(companyId, pickListId, page, pageSize), cancellationToken);
        return Ok(result);
    }
}

public sealed record CreatePackageLineRequest(Guid ProductId, decimal Quantity);
public sealed record CreatePackageRequest(
    Guid CompanyId,
    string PackageNumber,
    decimal? WeightKg,
    decimal? LengthCm,
    decimal? WidthCm,
    decimal? HeightCm,
    IReadOnlyList<CreatePackageLineRequest> Lines);
