using FusionOS.Modules.Crm.Application.Opportunities.Commands.AssignOpportunityAccount;
using FusionOS.Modules.Crm.Application.Opportunities.Commands.AssignOpportunityOwner;
using FusionOS.Modules.Crm.Application.Opportunities.Commands.ChangeOpportunityStage;
using FusionOS.Modules.Crm.Application.Opportunities.Commands.CreateOpportunity;
using FusionOS.Modules.Crm.Application.Opportunities.Commands.LoseOpportunity;
using FusionOS.Modules.Crm.Application.Opportunities.Commands.UpdateOpportunityForecast;
using FusionOS.Modules.Crm.Application.Opportunities.Commands.WinOpportunity;
using FusionOS.Modules.Crm.Application.Opportunities.Queries.GetOpportunityById;
using FusionOS.Modules.Crm.Application.Opportunities.Queries.ListOpportunities;
using FusionOS.Modules.Crm.Domain.Opportunities;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Crm.Api.Controllers;

/// <summary>
/// Phase 4 — CRM: Opportunities. A deal opened from a qualified lead, with an Open → Won /
/// Lost pipeline. Winning raises OpportunityWon, which Sales consumes to create the real
/// Customer (using the code supplied on the win request).
/// </summary>
[ApiController]
[Route("api/v1/crm/opportunities")]
public sealed class OpportunitiesController : ControllerBase
{
    private readonly ISender _sender;

    public OpportunitiesController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateOpportunityRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateOpportunityCommand(request.CompanyId, request.LeadId, request.Name, request.EstimatedValue, request.Probability, request.ExpectedCloseDate);
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id, companyId = request.CompanyId }, result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetOpportunityByIdQuery(companyId, id), cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] Guid companyId, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListOpportunitiesQuery(companyId, page, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/win")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Win(Guid id, [FromBody] WinOpportunityRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new WinOpportunityCommand(request.CompanyId, id, request.CustomerCode), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/lose")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Lose(Guid id, [FromBody] OpportunityActionRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new LoseOpportunityCommand(request.CompanyId, id), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/assign-account")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignAccount(Guid id, [FromBody] AssignOpportunityAccountRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new AssignOpportunityAccountCommand(request.CompanyId, id, request.AccountId), cancellationToken);
        return Ok(result);
    }

    /// <summary>Moves the deal between the non-terminal pipeline stages (Open/Prospecting/Qualification/Proposal/Negotiation) — use /win or /lose to close it instead.</summary>
    [HttpPost("{id:guid}/change-stage")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangeStage(Guid id, [FromBody] ChangeOpportunityStageRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ChangeOpportunityStageCommand(request.CompanyId, id, request.Stage), cancellationToken);
        return Ok(result);
    }

    /// <summary>Sets or clears the forecast fields (Probability/ExpectedCloseDate) on an in-flight opportunity.</summary>
    [HttpPost("{id:guid}/forecast")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateForecast(Guid id, [FromBody] UpdateOpportunityForecastRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new UpdateOpportunityForecastCommand(request.CompanyId, id, request.Probability, request.ExpectedCloseDate), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/assign-owner")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignOwner(Guid id, [FromBody] AssignOpportunityOwnerRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new AssignOpportunityOwnerCommand(request.CompanyId, id, request.OwnerId), cancellationToken);
        return Ok(result);
    }
}

public sealed record CreateOpportunityRequest(Guid CompanyId, Guid LeadId, string Name, decimal EstimatedValue, decimal? Probability = null, DateTimeOffset? ExpectedCloseDate = null);

public sealed record WinOpportunityRequest(Guid CompanyId, string CustomerCode);

public sealed record OpportunityActionRequest(Guid CompanyId);

public sealed record AssignOpportunityAccountRequest(Guid CompanyId, Guid? AccountId);

public sealed record ChangeOpportunityStageRequest(Guid CompanyId, OpportunityStage Stage);

public sealed record UpdateOpportunityForecastRequest(Guid CompanyId, decimal? Probability, DateTimeOffset? ExpectedCloseDate);

public sealed record AssignOpportunityOwnerRequest(Guid CompanyId, Guid? OwnerId);
