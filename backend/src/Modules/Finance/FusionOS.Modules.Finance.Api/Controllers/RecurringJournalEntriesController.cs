using FusionOS.Modules.Finance.Application.RecurringJournalEntries.Commands.CreateRecurringJournalEntryTemplate;
using FusionOS.Modules.Finance.Application.RecurringJournalEntries.Commands.DeactivateRecurringJournalEntryTemplate;
using FusionOS.Modules.Finance.Application.RecurringJournalEntries.Commands.GenerateJournalEntryFromTemplate;
using FusionOS.Modules.Finance.Application.RecurringJournalEntries.Commands.UpdateRecurringJournalEntryTemplate;
using FusionOS.Modules.Finance.Application.RecurringJournalEntries.Queries.GetRecurringJournalEntryTemplateById;
using FusionOS.Modules.Finance.Application.RecurringJournalEntries.Queries.ListRecurringJournalEntryTemplates;
using FusionOS.Modules.Finance.Domain.RecurringJournalEntries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Finance.Api.Controllers;

/// <summary>
/// New — recurring journal entry templates (a monthly rent/depreciation-style entry that
/// should repeat). Creating/updating a template never itself posts anything; only
/// POST {id}/generate turns its current lines into a real JournalEntry (going through the exact
/// same validation JournalEntriesController's Create does — see
/// GenerateJournalEntryFromTemplateCommandHandler's own doc comment) and advances NextDueDate.
/// </summary>
[ApiController]
[Route("api/v1/finance/recurring-journal-entries")]
public sealed class RecurringJournalEntriesController : ControllerBase
{
    private readonly ISender _sender;

    public RecurringJournalEntriesController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateRecurringJournalEntryTemplateRequest request, CancellationToken cancellationToken)
    {
        var lines = request.Lines.Select(l => new RecurringJournalEntryTemplateLineInput(l.AccountId, l.Debit, l.Credit, l.CostCenterId)).ToList();
        var command = new CreateRecurringJournalEntryTemplateCommand(request.CompanyId, request.Reference, request.Frequency, request.NextDueDate, lines);
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id, companyId = request.CompanyId }, result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetRecurringJournalEntryTemplateByIdQuery(companyId, id), cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] Guid companyId, [FromQuery] bool dueNow = false, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListRecurringJournalEntryTemplatesQuery(companyId, dueNow, page, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRecurringJournalEntryTemplateRequest request, CancellationToken cancellationToken)
    {
        var lines = request.Lines.Select(l => new RecurringJournalEntryTemplateLineInput(l.AccountId, l.Debit, l.Credit, l.CostCenterId)).ToList();
        var command = new UpdateRecurringJournalEntryTemplateCommand(request.CompanyId, id, request.Reference, request.Frequency, request.NextDueDate, lines);
        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(Guid id, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeactivateRecurringJournalEntryTemplateCommand(companyId, id), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/generate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Generate(Guid id, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GenerateJournalEntryFromTemplateCommand(companyId, id), cancellationToken);
        return Ok(result);
    }
}

public sealed record RecurringJournalEntryTemplateLineRequest(Guid AccountId, decimal Debit, decimal Credit, Guid? CostCenterId = null);

public sealed record CreateRecurringJournalEntryTemplateRequest(Guid CompanyId, string Reference, RecurringJournalEntryFrequency Frequency, DateTimeOffset NextDueDate, IReadOnlyList<RecurringJournalEntryTemplateLineRequest> Lines);

public sealed record UpdateRecurringJournalEntryTemplateRequest(Guid CompanyId, string Reference, RecurringJournalEntryFrequency Frequency, DateTimeOffset NextDueDate, IReadOnlyList<RecurringJournalEntryTemplateLineRequest> Lines);
