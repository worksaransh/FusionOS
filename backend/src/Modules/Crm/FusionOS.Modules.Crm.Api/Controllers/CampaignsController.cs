using FusionOS.Modules.Crm.Application.Campaigns.Commands.AddCampaignMember;
using FusionOS.Modules.Crm.Application.Campaigns.Commands.CreateCampaign;
using FusionOS.Modules.Crm.Application.Campaigns.Commands.DeactivateCampaign;
using FusionOS.Modules.Crm.Application.Campaigns.Commands.RemoveCampaignMember;
using FusionOS.Modules.Crm.Application.Campaigns.Queries.GetCampaignById;
using FusionOS.Modules.Crm.Application.Campaigns.Queries.ListCampaigns;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Crm.Api.Controllers;

/// <summary>Phase 7 — CRM depth: marketing campaigns and lead/contact membership.</summary>
[ApiController]
[Route("api/v1/crm/campaigns")]
public sealed class CampaignsController : ControllerBase
{
    private readonly ISender _sender;

    public CampaignsController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateCampaignRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateCampaignCommand(request.CompanyId, request.Name, request.Description, request.StartDate, request.EndDate);
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id, companyId = request.CompanyId }, result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetCampaignByIdQuery(companyId, id), cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] Guid companyId, [FromQuery] string? search = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListCampaignsQuery(companyId, search, page, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(Guid id, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeactivateCampaignCommand(companyId, id), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/members")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddMember(Guid id, [FromBody] AddCampaignMemberRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new AddCampaignMemberCommand(request.CompanyId, id, request.MemberEntityType, request.MemberEntityId), cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}/members")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveMember(Guid id, [FromQuery] Guid companyId, [FromQuery] string memberEntityType, [FromQuery] Guid memberEntityId, CancellationToken cancellationToken)
    {
        await _sender.Send(new RemoveCampaignMemberCommand(companyId, id, memberEntityType, memberEntityId), cancellationToken);
        return NoContent();
    }
}

public sealed record CreateCampaignRequest(Guid CompanyId, string Name, string? Description, DateTimeOffset StartDate, DateTimeOffset? EndDate);

public sealed record AddCampaignMemberRequest(Guid CompanyId, string MemberEntityType, Guid MemberEntityId);
