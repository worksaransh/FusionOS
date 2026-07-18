using FusionOS.Modules.Finance.Application.CostCenters.Commands.CreateCostCenter;
using FusionOS.Modules.Finance.Application.CostCenters.Commands.DeactivateCostCenter;
using FusionOS.Modules.Finance.Application.CostCenters.Commands.UpdateCostCenter;
using FusionOS.Modules.Finance.Application.CostCenters.Queries.GetCostCenterById;
using FusionOS.Modules.Finance.Application.CostCenters.Queries.ListCostCenters;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Finance.Api.Controllers;

/// <summary>M8a — Finance depth: Cost Centers. Pure master data, same shape as AccountsController minus the AccountType/ParentAccountId fields.</summary>
[ApiController]
[Route("api/v1/finance/cost-centers")]
public sealed class CostCentersController : ControllerBase
{
    private readonly ISender _sender;

    public CostCentersController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateCostCenterRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateCostCenterCommand(request.CompanyId, request.Code, request.Name);
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id, companyId = request.CompanyId }, result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetCostCenterByIdQuery(companyId, id), cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] Guid companyId, [FromQuery] string? search = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListCostCentersQuery(companyId, search, page, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCostCenterRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateCostCenterCommand(request.CompanyId, id, request.Name);
        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }

    // Soft-deactivate only — never a real delete (04_DATABASE_GUIDELINES.md). Uses
    // a body-bound request like ProductsController/DeactivateProductRequest rather
    // than AccountsController's [FromQuery] companyId, so the request shape matches
    // what apiClient.post's callers actually send (a JSON body), not a query string.
    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(Guid id, [FromBody] DeactivateCostCenterRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeactivateCostCenterCommand(request.CompanyId, id), cancellationToken);
        return Ok(result);
    }
}

public sealed record CreateCostCenterRequest(Guid CompanyId, string Code, string Name);

public sealed record UpdateCostCenterRequest(Guid CompanyId, string Name);

public sealed record DeactivateCostCenterRequest(Guid CompanyId);
