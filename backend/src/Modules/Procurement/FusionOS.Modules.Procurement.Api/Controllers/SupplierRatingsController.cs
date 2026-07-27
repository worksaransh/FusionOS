using FusionOS.Modules.Procurement.Application.SupplierRatings.Commands.CreateSupplierRating;
using FusionOS.Modules.Procurement.Application.SupplierRatings.Queries.GetSupplierRatingById;
using FusionOS.Modules.Procurement.Application.SupplierRatings.Queries.ListSupplierRatingsBySupplier;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Procurement.Api.Controllers;

/// <summary>
/// Manually-recorded supplier performance ratings — a genuine human judgment
/// (quality/delivery, 1-5, optional comments), distinct from the purely
/// PO-derived GetSupplierScorecardReport (ReportsController's supplier-scorecard
/// endpoint), which this feature's averages now feed into. A rating may
/// optionally reference the PurchaseOrder it followed; CreateSupplierRatingCommandHandler
/// enforces that such a PO belongs to the same supplier and is FullyReceived.
/// </summary>
[ApiController]
[Route("api/v1/procurement/supplier-ratings")]
public sealed class SupplierRatingsController : ControllerBase
{
    private readonly ISender _sender;

    public SupplierRatingsController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateSupplierRatingCommand command, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id, companyId = command.CompanyId }, result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetSupplierRatingByIdQuery(companyId, id), cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] Guid companyId, [FromQuery] Guid supplierId, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListSupplierRatingsBySupplierQuery(companyId, supplierId, page, pageSize), cancellationToken);
        return Ok(result);
    }
}
