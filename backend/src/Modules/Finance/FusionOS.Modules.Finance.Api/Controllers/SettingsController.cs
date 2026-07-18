using FusionOS.Modules.Finance.Application.Settings.Commands.UpdateFinanceSettings;
using FusionOS.Modules.Finance.Application.Settings.Queries.GetFinanceSettings;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Finance.Api.Controllers;

/// <summary>
/// Finance's own default-account settings (Phase 2 closeout, 2026-07-18) —
/// the Chart-of-Accounts mapping InvoiceIssuedConsumer/CreditNoteIssuedConsumer/
/// PurchaseOrderGoodsReceiptCostedConsumer use to auto-post a GL entry
/// alongside the AR/AP subledger entry they already write. GET always
/// returns a row — GetFinanceSettingsQueryHandler creates defaults (all
/// account ids unset) on first read, same "no separate bootstrap step"
/// convention as Core's own CompanySettings.
/// </summary>
[ApiController]
[Route("api/v1/finance/settings")]
public sealed class SettingsController : ControllerBase
{
    private readonly ISender _sender;

    public SettingsController(ISender sender) => _sender = sender;

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Get([FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetFinanceSettingsQuery(companyId), cancellationToken);
        return Ok(result);
    }

    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Update([FromBody] UpdateFinanceSettingsRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateFinanceSettingsCommand(
            request.CompanyId, request.DefaultArAccountId, request.DefaultSalesRevenueAccountId,
            request.DefaultApAccountId, request.DefaultPurchaseExpenseAccountId);
        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }
}

public sealed record UpdateFinanceSettingsRequest(
    Guid CompanyId,
    Guid? DefaultArAccountId,
    Guid? DefaultSalesRevenueAccountId,
    Guid? DefaultApAccountId,
    Guid? DefaultPurchaseExpenseAccountId);
