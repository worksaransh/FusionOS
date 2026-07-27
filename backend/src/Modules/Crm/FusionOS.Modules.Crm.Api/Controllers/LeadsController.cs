using FusionOS.Modules.Crm.Application.Leads.Commands.AssignLeadAccount;
using FusionOS.Modules.Crm.Application.Leads.Commands.AssignLeadOwner;
using FusionOS.Modules.Crm.Application.Leads.Commands.CreateLead;
using FusionOS.Modules.Crm.Application.Leads.Commands.DisqualifyLead;
using FusionOS.Modules.Crm.Application.Leads.Commands.QualifyLead;
using FusionOS.Modules.Crm.Application.Leads.Commands.RecalculateLeadScore;
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

    /// <summary>Creates a new lead in the New stage for the given company.</summary>
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

    /// <summary>Gets a single lead by id, scoped to the given company (companyId is required, not inferred from the lead).</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetLeadByIdQuery(companyId, id), cancellationToken);
        return Ok(result);
    }

    /// <summary>Lists leads for a company with optional free-text search, paginated (default page size 25).</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] Guid companyId, [FromQuery] string? search = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListLeadsQuery(companyId, search, page, pageSize), cancellationToken);
        return Ok(result);
    }

    /// <summary>Moves the lead to Qualified, making it eligible to be converted into an Opportunity.</summary>
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

    /// <summary>Moves the lead to Disqualified, ending its lifecycle without creating an Opportunity.</summary>
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

    /// <summary>Links (or clears, if AccountId is null) the CRM Account this lead belongs to.</summary>
    [HttpPost("{id:guid}/assign-account")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignAccount(Guid id, [FromBody] AssignLeadAccountRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new AssignLeadAccountCommand(request.CompanyId, id, request.AccountId), cancellationToken);
        return Ok(result);
    }

    /// <summary>Assigns (or clears, if OwnerId is null) the user responsible for working this lead.</summary>
    [HttpPost("{id:guid}/assign-owner")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignOwner(Guid id, [FromBody] AssignLeadOwnerRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new AssignLeadOwnerCommand(request.CompanyId, id, request.OwnerId), cancellationToken);
        return Ok(result);
    }

    /// <summary>Re-runs LeadScoringService against this lead's current AccountId/logged Activities — see RecalculateLeadScoreCommand for why "crm.lead.qualify" is the permission reused here.</summary>
    [HttpPost("{id:guid}/recalculate-score")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RecalculateScore(Guid id, [FromBody] LeadActionRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new RecalculateLeadScoreCommand(request.CompanyId, id), cancellationToken);
        return Ok(result);
    }
}

public sealed record CreateLeadRequest(Guid CompanyId, string Name, string? ContactEmail, string? ContactPhone, string? Source);

public sealed record LeadActionRequest(Guid CompanyId);

public sealed record AssignLeadAccountRequest(Guid CompanyId, Guid? AccountId);

public sealed record AssignLeadOwnerRequest(Guid CompanyId, Guid? OwnerId);
