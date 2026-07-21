using FusionOS.Modules.Core.Application.Branches.Commands.CreateBranch;
using FusionOS.Modules.Core.Application.Branches.Commands.DeactivateBranch;
using FusionOS.Modules.Core.Application.Branches.Commands.UpdateBranch;
using FusionOS.Modules.Core.Application.Branches.Queries.GetBranchById;
using FusionOS.Modules.Core.Application.Branches.Queries.ListBranches;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Core.Api.Controllers;

/// <summary>Branches/locations under a Company — pure master data, same shape as Finance's CostCentersController.</summary>
[ApiController]
[Route("api/v1/core/branches")]
public sealed class BranchesController : ControllerBase
{
    private readonly ISender _sender;

    public BranchesController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateBranchRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateBranchCommand(request.CompanyId, request.Name, request.Code, request.IsHeadOffice);
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id, companyId = request.CompanyId }, result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetBranchByIdQuery(companyId, id), cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] Guid companyId, [FromQuery] string? search = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListBranchesQuery(companyId, search, page, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBranchRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateBranchCommand(request.CompanyId, id, request.Name, request.IsHeadOffice);
        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }

    // Soft-deactivate only — never a real delete (04_DATABASE_GUIDELINES.md). Uses
    // a body-bound request like CostCentersController/ProductsController rather
    // than [FromQuery] companyId, so the request shape matches what apiClient.post's
    // callers actually send (a JSON body).
    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(Guid id, [FromBody] DeactivateBranchRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeactivateBranchCommand(request.CompanyId, id), cancellationToken);
        return Ok(result);
    }
}

public sealed record CreateBranchRequest(Guid CompanyId, string Name, string Code, bool IsHeadOffice = false);

public sealed record UpdateBranchRequest(Guid CompanyId, string Name, bool IsHeadOffice);

public sealed record DeactivateBranchRequest(Guid CompanyId);
