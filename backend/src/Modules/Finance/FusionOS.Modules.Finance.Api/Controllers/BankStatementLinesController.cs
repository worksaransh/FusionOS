using FusionOS.Modules.Finance.Application.BankStatementLines.Commands.ReconcileStatementLine;
using FusionOS.Modules.Finance.Application.BankStatementLines.Commands.RecordStatementLine;
using FusionOS.Modules.Finance.Application.BankStatementLines.Commands.UnreconcileStatementLine;
using FusionOS.Modules.Finance.Application.BankStatementLines.Queries.GetReconciliationSummary;
using FusionOS.Modules.Finance.Application.BankStatementLines.Queries.ListBankStatementLines;
using FusionOS.Modules.Finance.Application.BankStatementLines.Queries.SuggestMatchesForStatementLine;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Finance.Api.Controllers;

/// <summary>
/// M8d — Finance depth: bank reconciliation. Statement lines nest under a
/// bank account via the path, same convention as Warehouse's Bins nesting
/// under Warehouse/Zone (`BinsController`). No bank-feed/file-import
/// endpoint here — every line is entered one at a time via `POST` — and no
/// auto-matching endpoint: `Reconcile` always takes an optional, user-picked
/// `MatchedJournalEntryId` (see `BankStatementLine`'s class doc comment for
/// both scope-outs).
/// </summary>
[ApiController]
[Route("api/v1/finance/bank-accounts/{bankAccountId:guid}/statement-lines")]
public sealed class BankStatementLinesController : ControllerBase
{
    private readonly ISender _sender;

    public BankStatementLinesController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create(Guid bankAccountId, [FromBody] RecordStatementLineRequest request, CancellationToken cancellationToken)
    {
        var command = new RecordStatementLineCommand(request.CompanyId, bankAccountId, request.TransactionDate, request.Amount, request.Description);
        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List(Guid bankAccountId, [FromQuery] Guid companyId, [FromQuery] bool? isReconciled = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListBankStatementLinesQuery(companyId, bankAccountId, isReconciled, page, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpGet("summary")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSummary(Guid bankAccountId, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetReconciliationSummaryQuery(companyId, bankAccountId), cancellationToken);
        return Ok(result);
    }

    // Read-only match suggestions for one statement line — same-amount, within a
    // small date window. Only proposes candidates; the user still confirms via
    // reconcile (see BankStatementLine's class doc comment on "no auto-matching").
    [HttpGet("{id:guid}/match-suggestions")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MatchSuggestions(Guid bankAccountId, Guid id, [FromQuery] Guid companyId, [FromQuery] int dateToleranceDays = 3, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new SuggestMatchesForStatementLineQuery(companyId, id, dateToleranceDays), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/reconcile")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reconcile(Guid bankAccountId, Guid id, [FromBody] ReconcileStatementLineRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ReconcileStatementLineCommand(request.CompanyId, id, request.MatchedJournalEntryId), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/unreconcile")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Unreconcile(Guid bankAccountId, Guid id, [FromBody] UnreconcileStatementLineRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new UnreconcileStatementLineCommand(request.CompanyId, id), cancellationToken);
        return Ok(result);
    }
}

public sealed record RecordStatementLineRequest(Guid CompanyId, DateTimeOffset TransactionDate, decimal Amount, string Description);

public sealed record ReconcileStatementLineRequest(Guid CompanyId, Guid? MatchedJournalEntryId);

public sealed record UnreconcileStatementLineRequest(Guid CompanyId);
