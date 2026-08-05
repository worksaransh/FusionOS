using FusionOS.Modules.Finance.Application.WithholdingTaxTransactions.Commands.RecordWithholdingTaxDeduction;
using FusionOS.Modules.Finance.Application.WithholdingTaxTransactions.Queries.ListWithholdingTaxTransactions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Finance.Api.Controllers;

/// <summary>
/// TDS/TCS deduction records — see RecordWithholdingTaxDeductionCommandHandler's doc comment
/// for the exact GL double-entry this posts and the deliberate scope-out (TDS on payments to
/// suppliers only; TCS-on-receivables is a documented follow-up).
/// </summary>
[ApiController]
[Route("api/v1/finance/withholding-tax-transactions")]
public sealed class WithholdingTaxTransactionsController : ControllerBase
{
    private readonly ISender _sender;

    public WithholdingTaxTransactionsController(ISender sender) => _sender = sender;

    [HttpPost("record-deduction")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RecordDeduction([FromBody] RecordWithholdingTaxDeductionRequest request, CancellationToken cancellationToken)
    {
        var command = new RecordWithholdingTaxDeductionCommand(
            request.CompanyId, request.WithholdingTaxRateId, request.ReferenceType, request.ReferenceId,
            request.GrossAmount, request.PayableGlAccountId, request.WithholdingPayableGlAccountId);
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(List), new { companyId = request.CompanyId }, result);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] Guid companyId, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListWithholdingTaxTransactionsQuery(companyId, page, pageSize), cancellationToken);
        return Ok(result);
    }
}

public sealed record RecordWithholdingTaxDeductionRequest(
    Guid CompanyId,
    Guid WithholdingTaxRateId,
    string ReferenceType,
    Guid ReferenceId,
    decimal GrossAmount,
    Guid PayableGlAccountId,
    Guid WithholdingPayableGlAccountId);
