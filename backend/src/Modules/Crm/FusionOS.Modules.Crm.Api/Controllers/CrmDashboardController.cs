using FusionOS.Modules.Crm.Application.Dashboard.Queries.GetCrmDashboard;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Crm.Api.Controllers;

/// <summary>
/// Phase 4 — CRM depth pass: the canned pipeline/forecast/conversion dashboard. Read-only,
/// gated on crm.dashboard.read — a new controller rather than folding into
/// OpportunitiesController/LeadsController since it spans both aggregates and isn't scoped
/// to a single entity's CRUD lifecycle, same reasoning Finance uses for its own
/// ReportsController sitting alongside PayablesController/ReceivablesController.
/// </summary>
[ApiController]
[Route("api/v1/crm/dashboard")]
public sealed class CrmDashboardController : ControllerBase
{
    private readonly ISender _sender;

    public CrmDashboardController(ISender sender) => _sender = sender;

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Get([FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetCrmDashboardQuery(companyId), cancellationToken);
        return Ok(result);
    }
}
