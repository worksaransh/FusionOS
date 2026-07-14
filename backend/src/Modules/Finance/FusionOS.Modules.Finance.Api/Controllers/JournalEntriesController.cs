using FusionOS.Modules.Finance.Application.JournalEntries.Commands.CreateJournalEntry;
using FusionOS.Modules.Finance.Application.JournalEntries.Commands.PostJournalEntry;
using FusionOS.Modules.Finance.Application.JournalEntries.Queries.ListJournalEntries;
using FusionOS.Modules.Finance.Domain.JournalEntries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Finance.Api.Controllers;

/// <summary>General Ledger journal entries — the second Phase 2 slice, built on Accounts.</summary>
[ApiController]
[Route("api/v1/finance/journal-entries")]
public sealed class JournalEntriesController : ControllerBase
{
    private readonly ISender _sender;

    public JournalEntriesController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateJournalEntryRequest request, CancellationToken cancellationToken)
    {
        var lines = request.Lines.Select(l => new JournalEntryLineInput(l.AccountId, l.Debit, l.Credit, l.Description)).ToList();
        var command = new CreateJournalEntryCommand(request.CompanyId, request.Reference, lines);
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(List), new { companyId = request.CompanyId }, result);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] Guid companyId, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListJournalEntriesQuery(companyId, page, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/post")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Post(Guid id, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new PostJournalEntryCommand(companyId, id), cancellationToken);
        return Ok(result);
    }
}

public sealed record CreateJournalEntryLineRequest(Guid AccountId, decimal Debit, decimal Credit, string? Description);

public sealed record CreateJournalEntryRequest(Guid CompanyId, string? Reference, IReadOnlyList<CreateJournalEntryLineRequest> Lines);
