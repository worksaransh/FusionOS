using FusionOS.Modules.Finance.Application.Receivables.Queries.GetCustomerBalance;
using FusionOS.Modules.Finance.Application.Receivables.Queries.ListArLedgerEntries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Finance.Api.Controllers;

/// <summary>Minimal Accounts Receivable slice — 05_MODULE_ROADMAP.md Phase 2. Kept in sync by InvoiceIssuedConsumer; nothing here writes to the ledger directly.</summary>
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
}
