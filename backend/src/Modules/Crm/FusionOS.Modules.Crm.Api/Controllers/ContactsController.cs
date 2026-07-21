using FusionOS.Modules.Crm.Application.Contacts.Commands.CreateContact;
using FusionOS.Modules.Crm.Application.Contacts.Commands.DeactivateContact;
using FusionOS.Modules.Crm.Application.Contacts.Commands.UpdateContact;
using FusionOS.Modules.Crm.Application.Contacts.Queries.GetContactById;
using FusionOS.Modules.Crm.Application.Contacts.Queries.ListContacts;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Crm.Api.Controllers;

/// <summary>
/// Phase 4 — CRM: Contacts. A named individual — usually belongs to an Account, but
/// can be captured straight off a Lead before an Account exists (see Contact.cs).
/// </summary>
[ApiController]
[Route("api/v1/crm/contacts")]
public sealed class ContactsController : ControllerBase
{
    private readonly ISender _sender;

    public ContactsController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateContactRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateContactCommand(request.CompanyId, request.Name, request.Email, request.Phone, request.Title, request.AccountId, request.LeadId);
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id, companyId = request.CompanyId }, result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetContactByIdQuery(companyId, id), cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] Guid companyId, [FromQuery] string? search = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListContactsQuery(companyId, search, page, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateContactRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateContactCommand(request.CompanyId, id, request.Name, request.Email, request.Phone, request.Title, request.AccountId, request.LeadId);
        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(Guid id, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeactivateContactCommand(companyId, id), cancellationToken);
        return Ok(result);
    }
}

public sealed record CreateContactRequest(Guid CompanyId, string Name, string? Email, string? Phone, string? Title, Guid? AccountId, Guid? LeadId);

public sealed record UpdateContactRequest(Guid CompanyId, string Name, string? Email, string? Phone, string? Title, Guid? AccountId, Guid? LeadId);
