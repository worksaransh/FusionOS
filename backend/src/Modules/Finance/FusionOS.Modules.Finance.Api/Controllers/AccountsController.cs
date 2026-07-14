using FusionOS.Modules.Finance.Application.Accounts.Commands.CreateAccount;
using FusionOS.Modules.Finance.Application.Accounts.Queries.ListAccounts;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Finance.Api.Controllers;

/// <summary>Chart of Accounts — the first Phase 2 slice (05_MODULE_ROADMAP.md).</summary>
[ApiController]
[Route("api/v1/finance/accounts")]
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
        var command = new CreateAccountCommand(request.CompanyId, request.Code, request.Name, request.AccountType, request.ParentAccountId);
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(List), new { companyId = request.CompanyId }, result);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] Guid companyId, [FromQuery] string? search = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListAccountsQuery(companyId, search, page, pageSize), cancellationToken);
        return Ok(result);
    }
}

public sealed record CreateAccountRequest(Guid CompanyId, string Code, string Name, string AccountType, Guid? ParentAccountId);
