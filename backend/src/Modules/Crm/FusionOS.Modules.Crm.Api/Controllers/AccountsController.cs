using FusionOS.Modules.Crm.Application.Accounts.Commands.CreateAccount;
using FusionOS.Modules.Crm.Application.Accounts.Commands.DeactivateAccount;
using FusionOS.Modules.Crm.Application.Accounts.Commands.UpdateAccount;
using FusionOS.Modules.Crm.Application.Accounts.Queries.GetAccountById;
using FusionOS.Modules.Crm.Application.Accounts.Queries.ListAccounts;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Crm.Api.Controllers;

/// <summary>
/// Phase 4 — CRM: Accounts. The organization/company a Lead, Opportunity, or Contact
/// belongs to — a pre-sales concept, distinct from Sales' Customer (see Account.cs).
/// </summary>
[ApiController]
[Route("api/v1/crm/accounts")]
public sealed class AccountsController : ControllerBase
{
    private readonly ISender _sender;

    public AccountsController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateAccountRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateAccountCommand(request.CompanyId, request.Name, request.Industry, request.Website);
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id, companyId = request.CompanyId }, result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetAccountByIdQuery(companyId, id), cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] Guid companyId, [FromQuery] string? search = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListAccountsQuery(companyId, search, page, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAccountRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateAccountCommand(request.CompanyId, id, request.Name, request.Industry, request.Website);
        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(Guid id, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeactivateAccountCommand(companyId, id), cancellationToken);
        return Ok(result);
    }
}

public sealed record CreateAccountRequest(Guid CompanyId, string Name, string? Industry, string? Website);

public sealed record UpdateAccountRequest(Guid CompanyId, string Name, string? Industry, string? Website);
