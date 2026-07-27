using FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Contracts;
using FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Queries.GenerateMrpRequirements;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Manufacturing.Api.Controllers;

/// <summary>
/// Phase 6 — Material Requirements Planning. Not a bill-of-materials resource action (it
/// plans across a whole demand list, potentially spanning many products/BOMs at once), so
/// this gets its own controller/route rather than living on BillsOfMaterialsController. See
/// GenerateMrpRequirementsQuery's doc comment for the honest "gross requirements only, not
/// netted against stock/supply" scope of what this runs.
/// </summary>
[ApiController]
[Route("api/v1/manufacturing/mrp")]
public sealed class MrpController : ControllerBase
{
    private readonly ISender _sender;

    public MrpController(ISender sender) => _sender = sender;

    /// <summary>
    /// Runs a gross-requirements MRP pass over the given demand list. POST because the body
    /// (a list of demand lines) doesn't fit a query string — this is a read-only planning
    /// calculation, not a persisted resource; nothing is created or stored.
    /// </summary>
    [HttpPost("run")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Run([FromBody] GenerateMrpRequirementsRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GenerateMrpRequirementsQuery(request.CompanyId, request.DemandLines), cancellationToken);
        return Ok(result);
    }
}

public sealed record GenerateMrpRequirementsRequest(Guid CompanyId, IReadOnlyList<MrpDemandLine> DemandLines);
