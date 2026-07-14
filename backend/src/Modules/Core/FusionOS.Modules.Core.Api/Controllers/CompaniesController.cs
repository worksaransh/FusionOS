using FusionOS.Modules.Core.Application.Companies.Commands.CreateCompany;
using FusionOS.Modules.Core.Application.Companies.Queries.ListCompanies;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Core.Api.Controllers;

/// <summary>
/// The one fully wired-up endpoint set for Phase 0 — versioned, paginated, validated
/// per 08_API_STANDARDS.md. Every other Core capability (Auth, RBAC administration,
/// Notifications, Workflow Engine, Settings, File Management, Search, Scheduler,
/// License Management) is scaffolded at the domain/persistence layer only and is
/// intentionally not exposed here yet — see docs/blueprint/05_MODULE_ROADMAP.md.
/// </summary>
[ApiController]
[Route("api/v1/core/companies")]
public sealed class CompaniesController : ControllerBase
{
    private readonly ISender _sender;

    public CompaniesController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateCompanyCommand command, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById(Guid id)
    {
        // NOTE: a dedicated GetCompanyByIdQuery is the natural next slice; omitted
        // here to keep this scaffold's one vertical slice narrow and honest.
        return NotFound();
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListCompaniesQuery(page, pageSize), cancellationToken);
        return Ok(result);
    }
}
