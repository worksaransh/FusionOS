using FusionOS.Modules.Crm.Application.Leads.Commands.CreateLead;
using FusionOS.Modules.Crm.Application.Leads.Commands.DisqualifyLead;
using FusionOS.Modules.Crm.Application.Leads.Commands.QualifyLead;
using FusionOS.Modules.Crm.Application.Leads.Queries.GetLeadById;
using FusionOS.Modules.Crm.Application.Leads.Queries.ListLeads;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Crm.Api.Controllers;

/// <summary>
/// Phase 4 — CRM: Leads. Raw prospects with a New → Qualified → Converted / Disqualified
/// lifecycle. A qualified lead is the starting point for an Opportunity (see
/// OpportunitiesController), which is what eventually creates a Sales Customer.
/// </summary>
[ApiController]
[Route("api/v1/crm/leads")]
public sealed class LeadsController : ControllerBase
{
    private readonly ISender _sender;

    public LeadsController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateLeadRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateLeadCommand(request.CompanyId, request.Name, request.ContactEmail, request.ContactPhone, request.Source);
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id, companyId = request.CompanyId }, result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetLeadByIdQuery(companyId, id), cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] Guid companyId, [FromQuery] string? search = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListLeadsQuery(companyId, search, page, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/qualify")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Qualify(Guid id, [FromBody] LeadActionRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new QualifyLeadCommand(request.CompanyId, id), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/disqualify")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Disqualify(Guid id, [FromBody] LeadActionRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DisqualifyLeadCommand(request.CompanyId, id), cancellationToken);
        return Ok(result);
    }
}

public sealed record CreateLeadRequest(Guid CompanyId, string Name, string? ContactEmail, string? ContactPhone, string? Source);

public sealed record LeadActionRequest(Guid CompanyId);
