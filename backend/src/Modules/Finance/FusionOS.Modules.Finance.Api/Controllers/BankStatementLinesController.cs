using System.Globalization;
using FusionOS.Modules.Finance.Application.BankStatementLines.Commands.ImportBankStatementLines;
using FusionOS.Modules.Finance.Application.BankStatementLines.Commands.ReconcileStatementLine;
using FusionOS.Modules.Finance.Application.BankStatementLines.Commands.RecordStatementLine;
using FusionOS.Modules.Finance.Application.BankStatementLines.Commands.UnreconcileStatementLine;
using FusionOS.Modules.Finance.Application.BankStatementLines.Contracts;
using FusionOS.Modules.Finance.Application.BankStatementLines.Queries.GetReconciliationSummary;
using FusionOS.Modules.Finance.Application.BankStatementLines.Queries.ListBankStatementLines;
using FusionOS.Modules.Finance.Application.BankStatementLines.Queries.SuggestMatchesForStatementLine;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Finance.Api.Controllers;

/// <summary>
/// M8d — Finance depth: bank reconciliation. Statement lines nest under a
/// bank account via the path, same convention as Warehouse's Bins nesting
/// under Warehouse/Zone (`BinsController`). Every line can still be entered
/// one at a time via `POST` — and there is still no auto-matching endpoint:
/// `Reconcile` always takes an optional, user-picked `MatchedJournalEntryId`
/// (see `BankStatementLine`'s class doc comment). As of the bulk-import
/// follow-up, `POST .../import` also accepts a small hand-rolled CSV upload
/// (see `Import`'s own doc comment for the exact, deliberately-limited
/// format supported) — CSV parsing lives here at the Api layer, same "the
/// controller reads the raw IFormFile, the command only ever sees already-
/// parsed rows" split DocumentsController.Upload uses for its own file
/// handling.
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

    /// <summary>
    /// Bulk-imports statement lines from an uploaded CSV file. Supported
    /// format, deliberately minimal (no external CSV package is referenced
    /// anywhere in this codebase, so this hand-rolls the simplest reasonable
    /// shape rather than risk an unproven quote/escaping-aware parser):
    /// a required header row, then one data row per line, exactly three
    /// comma-separated columns in this fixed order —
    /// <c>TransactionDate,Description,Amount</c>. TransactionDate must parse
    /// via <see cref="DateTimeOffset.Parse(string, IFormatProvider?)"/>
    /// (invariant culture; ISO-8601 recommended, e.g. 2026-07-20), Amount via
    /// <see cref="decimal.Parse(string, IFormatProvider?)"/>. There is no
    /// support for quoted fields or embedded commas/quotes within
    /// Description — a Description containing a comma will misparse the
    /// row, which is why this is documented rather than silently assumed.
    /// A row that fails to parse is reported back as an error and does not
    /// stop the rest of the file from importing (see
    /// BankStatementLineImportResultDto's own doc comment); a row that
    /// parses but duplicates an existing line for this bank account
    /// (same TransactionDate + Description + Amount) is silently skipped
    /// and counted, not re-imported.
    /// </summary>
    [HttpPost("import")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Import(Guid bankAccountId, [FromForm] Guid companyId, [FromForm] IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
            return BadRequest("A non-empty CSV file is required.");

        var (validRows, parseErrors, totalDataRows) = await ParseCsvAsync(file, cancellationToken);

        var command = new ImportBankStatementLinesCommand(companyId, bankAccountId, validRows);
        var result = await _sender.Send(command, cancellationToken);

        var mergedErrors = parseErrors.Concat(result.Errors).OrderBy(e => e.RowNumber).ToList();
        var merged = result with { RowsParsed = totalDataRows, Errors = mergedErrors };

        return Ok(merged);
    }

    /// <summary>
    /// Reads the uploaded file as text and splits it into rows — see
    /// <see cref="Import"/>'s doc comment for the exact supported format.
    /// The header row (first non-blank line) is always skipped, whatever
    /// its contents, rather than validated field-by-field.
    /// </summary>
    private static async Task<(List<ImportBankStatementLineInput> ValidRows, List<ImportBankStatementLineRowErrorDto> ParseErrors, int TotalDataRows)> ParseCsvAsync(IFormFile file, CancellationToken cancellationToken)
    {
        var validRows = new List<ImportBankStatementLineInput>();
        var parseErrors = new List<ImportBankStatementLineRowErrorDto>();

        using var reader = new StreamReader(file.OpenReadStream());
        var content = await reader.ReadToEndAsync(cancellationToken);
        var allLines = content.Split('\n').Select(l => l.TrimEnd('\r')).ToList();

        var dataLines = allLines.Where(l => !string.IsNullOrWhiteSpace(l)).ToList();
        if (dataLines.Count == 0)
            return (validRows, parseErrors, 0);

        // First non-blank line is always the header — skipped unconditionally.
        var rowNumber = 0;
        foreach (var line in dataLines.Skip(1))
        {
            rowNumber++;
            var fields = line.Split(',');

            if (fields.Length != 3)
            {
                parseErrors.Add(new ImportBankStatementLineRowErrorDto(rowNumber, $"Expected 3 comma-separated columns (TransactionDate,Description,Amount) but found {fields.Length}."));
                continue;
            }

            if (!DateTimeOffset.TryParse(fields[0].Trim(), CultureInfo.InvariantCulture, DateTimeStyles.None, out var transactionDate))
            {
                parseErrors.Add(new ImportBankStatementLineRowErrorDto(rowNumber, $"Could not parse '{fields[0].Trim()}' as a transaction date."));
                continue;
            }

            var description = fields[1].Trim();

            if (!decimal.TryParse(fields[2].Trim(), NumberStyles.Number | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var amount))
            {
                parseErrors.Add(new ImportBankStatementLineRowErrorDto(rowNumber, $"Could not parse '{fields[2].Trim()}' as an amount."));
                continue;
            }

            validRows.Add(new ImportBankStatementLineInput(rowNumber, transactionDate, amount, description));
        }

        return (validRows, parseErrors, dataLines.Count - 1);
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
