using FusionOS.Modules.Finance.Application.AccountingPeriods.Commands.CloseAccountingPeriod;
using FusionOS.Modules.Finance.Application.AccountingPeriods.Commands.CreateAccountingPeriod;
using FusionOS.Modules.Finance.Application.AccountingPeriods.Commands.ReopenAccountingPeriod;
using FusionOS.Modules.Finance.Application.AccountingPeriods.Queries.GetAccountingPeriodById;
using FusionOS.Modules.Finance.Application.AccountingPeriods.Queries.ListAccountingPeriods;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Finance.Api.Controllers;

/// <summary>
/// New — General Ledger period control. An AccountingPeriod is opt-in per company: creating
/// none, or none covering a given date, leaves JournalEntry posting exactly as unrestricted as
/// before this feature existed (see AccountingPeriod's and CreateJournalEntryCommandHandler's
/// own class/method doc comments). Only an explicitly Closed period blocks new postings into it.
/// </summary>
[ApiController]
[Route("api/v1/finance/accounting-periods")]
public sealed class AccountingPeriodsController : ControllerBase
{
    private readonly ISender _sender;

    public AccountingPeriodsController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateAccountingPeriodRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateAccountingPeriodCommand(request.CompanyId, request.PeriodStart, request.PeriodEnd);
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id, companyId = request.CompanyId }, result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetAccountingPeriodByIdQuery(companyId, id), cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] Guid companyId, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListAccountingPeriodsQuery(companyId, page, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/close")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Close(Guid id, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new CloseAccountingPeriodCommand(companyId, id), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/reopen")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reopen(Guid id, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ReopenAccountingPeriodCommand(companyId, id), cancellationToken);
        return Ok(result);
    }
}

public sealed record CreateAccountingPeriodRequest(Guid CompanyId, DateTimeOffset PeriodStart, DateTimeOffset PeriodEnd);
