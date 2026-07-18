using FusionOS.Modules.Finance.Application.Receivables.Commands.RecordPayment;
using FusionOS.Modules.Finance.Application.Receivables.Queries.GetCustomerBalance;
using FusionOS.Modules.Finance.Application.Receivables.Queries.ListArLedgerEntries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Finance.Api.Controllers;

/// <summary>
/// Minimal Accounts Receivable slice — 05_MODULE_ROADMAP.md Phase 2. Charge
/// entries are kept in sync by InvoiceIssuedConsumer; payment entries are
/// written directly here via RecordPaymentCommand (Phase M4, 2026-07-15) —
/// this is the one place in Receivables that writes to the ledger on its own,
/// since a customer payment has no corresponding integration event to react to.
/// </summary>
[ApiController]
[Route("api/v1/finance/receivables")]
public sealed class ReceivablesController : ControllerBase
{
    private readonly ISender _sender;

    public ReceivablesController(ISender sender) => _sender = sender;

    [HttpGet("balance")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBalance([FromQuery] Guid companyId, [FromQuery] Guid customerId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetCustomerBalanceQuery(companyId, customerId), cancellationToken);
        return Ok(result);
    }

    [HttpGet("ledger")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLedger([FromQuery] Guid companyId, [FromQuery] Guid customerId, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListArLedgerEntriesQuery(companyId, customerId, page, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpPost("payments")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RecordPayment([FromBody] RecordPaymentRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new RecordPaymentCommand(request.CompanyId, request.CustomerId, request.InvoiceId, request.Amount, request.PaymentDate, request.Reference),
            cancellationToken);
        return Ok(result);
    }
}

public sealed record RecordPaymentRequest(Guid CompanyId, Guid CustomerId, Guid InvoiceId, decimal Amount, DateTimeOffset? PaymentDate, string? Reference);
