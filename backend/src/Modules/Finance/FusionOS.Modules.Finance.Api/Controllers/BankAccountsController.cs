using FusionOS.Modules.Finance.Application.BankAccounts.Commands.CreateBankAccount;
using FusionOS.Modules.Finance.Application.BankAccounts.Commands.DeactivateBankAccount;
using FusionOS.Modules.Finance.Application.BankAccounts.Commands.UpdateBankAccount;
using FusionOS.Modules.Finance.Application.BankAccounts.Queries.GetBankAccountById;
using FusionOS.Modules.Finance.Application.BankAccounts.Queries.ListBankAccounts;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Finance.Api.Controllers;

/// <summary>M8d — Finance depth: bank reconciliation. BankAccount master data, same shape as CostCentersController plus a mandatory LinkedAccountId (the GL account this bank account reconciles against — see BankAccount.cs).</summary>
[ApiController]
[Route("api/v1/finance/bank-accounts")]
public sealed class BankAccountsController : ControllerBase
{
    private readonly ISender _sender;

    public BankAccountsController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateBankAccountRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateBankAccountCommand(request.CompanyId, request.Code, request.Name, request.LinkedAccountId, request.BankName, request.AccountNumberLast4);
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id, companyId = request.CompanyId }, result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetBankAccountByIdQuery(companyId, id), cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] Guid companyId, [FromQuery] string? search = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListBankAccountsQuery(companyId, search, page, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBankAccountRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateBankAccountCommand(request.CompanyId, id, request.Name, request.BankName, request.AccountNumberLast4);
        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }

    // Soft-deactivate only — never a real delete (04_DATABASE_GUIDELINES.md), same
    // body-bound-request convention as CostCentersController.Deactivate.
    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(Guid id, [FromBody] DeactivateBankAccountRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeactivateBankAccountCommand(request.CompanyId, id), cancellationToken);
        return Ok(result);
    }
}

public sealed record CreateBankAccountRequest(Guid CompanyId, string Code, string Name, Guid LinkedAccountId, string? BankName, string? AccountNumberLast4);

public sealed record UpdateBankAccountRequest(Guid CompanyId, string Name, string? BankName, string? AccountNumberLast4);

public sealed record DeactivateBankAccountRequest(Guid CompanyId);
