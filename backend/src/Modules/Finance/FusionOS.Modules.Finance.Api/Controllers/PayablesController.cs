using FusionOS.Modules.Finance.Application.Payables.Commands.RecordBillCharge;
using FusionOS.Modules.Finance.Application.Payables.Commands.RecordPayment;
using FusionOS.Modules.Finance.Application.Payables.Queries.GetSupplierBalance;
using FusionOS.Modules.Finance.Application.Payables.Queries.ListApLedgerEntries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Finance.Api.Controllers;

/// <summary>
/// Minimal Accounts Payable slice — Phase M8c (2026-07-17), the mirror image
/// of ReceivablesController. Both charge and payment entries are written
/// directly here — a bill charge via RecordBillChargeCommand, a payment via
/// RecordPaymentCommand — since, unlike AR's InvoiceIssuedConsumer, there is
/// no integration event yet that automatically creates an AP charge (see
/// ApLedgerEntry's class doc comment for the scope decision behind that).
/// </summary>
[ApiController]
[Route("api/v1/finance/payables")]
public sealed class PayablesController : ControllerBase
{
    private readonly ISender _sender;

    public PayablesController(ISender sender) => _sender = sender;

    [HttpGet("balance")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBalance([FromQuery] Guid companyId, [FromQuery] Guid supplierId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetSupplierBalanceQuery(companyId, supplierId), cancellationToken);
        return Ok(result);
    }

    [HttpGet("ledger")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLedger([FromQuery] Guid companyId, [FromQuery] Guid supplierId, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListApLedgerEntriesQuery(companyId, supplierId, page, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpPost("charges")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RecordBillCharge([FromBody] RecordBillChargeRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new RecordBillChargeCommand(request.CompanyId, request.SupplierId, request.PurchaseOrderId, request.Amount, request.Description),
            cancellationToken);
        return Ok(result);
    }

    [HttpPost("payments")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RecordPayment([FromBody] RecordApPaymentRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new RecordPaymentCommand(request.CompanyId, request.SupplierId, request.PurchaseOrderId, request.Amount, request.PaymentDate, request.Reference),
            cancellationToken);
        return Ok(result);
    }
}

public sealed record RecordBillChargeRequest(Guid CompanyId, Guid SupplierId, Guid? PurchaseOrderId, decimal Amount, string Description);

public sealed record RecordApPaymentRequest(Guid CompanyId, Guid SupplierId, Guid? PurchaseOrderId, decimal Amount, DateTimeOffset? PaymentDate, string? Reference);
