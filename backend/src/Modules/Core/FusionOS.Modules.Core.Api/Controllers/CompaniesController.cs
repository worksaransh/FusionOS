using FusionOS.Modules.Core.Application.Companies.Commands.CreateCompany;
using FusionOS.Modules.Core.Application.Companies.Commands.DeactivateCompany;
using FusionOS.Modules.Core.Application.Companies.Commands.UpdateCompany;
using FusionOS.Modules.Core.Application.Companies.Queries.GetCompanyById;
using FusionOS.Modules.Core.Application.Companies.Queries.ListCompanies;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Core.Api.Controllers;

/// <summary>
/// Companies administration. Create/List were the one fully wired-up
/// endpoint set for Phase 0 (08_API_STANDARDS.md). GetById/Update/Deactivate
/// were added in Phase I (2026-07-14 sprint) — see GetCompanyByIdQuery,
/// UpdateCompanyCommand, DeactivateCompanyCommand for the tenant-scoping
/// rationale (a Company is the tenant root, not a record within a tenant).
/// </summary>
[ApiController]
[Route("api/v1/core/companies")]
public sealed class CompaniesController : ControllerBase
{
    private readonly ISender _sender;

    public CompaniesController(ISender sender) => _sender = sender;

    // [AllowAnonymous] fixes a confirmed bootstrap deadlock: CreateCompanyCommand's
    // own handler has always been permission-free by design ("the bootstrap action
    // of a fresh tenant" — see CreateCompanyCommand.cs), matching how AuthController's
    // Register endpoint already allows an anonymous caller to create the first user
    // of a brand-new company. Before this attribute, the *endpoint* still required an
    // authenticated caller via the global FallbackPolicy (Program.cs), which meant a
    // fresh deployment with zero companies/users could never create its first tenant
    // at all: there was no way to become authenticated without a company to register
    // against, and no way to create a company without already being authenticated.
    [HttpPost]
    [AllowAnonymous]
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
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetCompanyByIdQuery(id), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListCompaniesQuery(page, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCompanyRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new UpdateCompanyCommand(id, request.Name, request.LegalName, request.TaxId), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeactivateCompanyCommand(id), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }
}

/// <summary>Body shape for PUT /companies/{id} — Id comes from the route, not the body.</summary>
public sealed record UpdateCompanyRequest(string Name, string LegalName, string? TaxId);
