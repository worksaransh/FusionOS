using FusionOS.Modules.Finance.Application.ExchangeRates.Commands.CreateExchangeRate;
using FusionOS.Modules.Finance.Application.ExchangeRates.Commands.DeactivateExchangeRate;
using FusionOS.Modules.Finance.Application.ExchangeRates.Commands.UpdateExchangeRate;
using FusionOS.Modules.Finance.Application.ExchangeRates.Queries.ConvertAmount;
using FusionOS.Modules.Finance.Application.ExchangeRates.Queries.GetExchangeRateById;
using FusionOS.Modules.Finance.Application.ExchangeRates.Queries.ListExchangeRates;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Finance.Api.Controllers;

/// <summary>
/// M8e — Finance depth: multi-currency support. Dated FX rate master data,
/// same shape as TaxRatesController/BankAccountsController, plus a `GET
/// .../convert` action wired to ConvertAmountQuery — the one bit of actual
/// currency-conversion behavior this slice ships. See ExchangeRate.cs's
/// class doc comment for the scope line: no existing aggregate has been
/// made currency-aware, this controller only manages rates and answers
/// "what would this amount convert to right now."
/// </summary>
[ApiController]
[Route("api/v1/finance/exchange-rates")]
public sealed class ExchangeRatesController : ControllerBase
{
    private readonly ISender _sender;

    public ExchangeRatesController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateExchangeRateRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateExchangeRateCommand(request.CompanyId, request.FromCurrencyCode, request.ToCurrencyCode, request.Rate, request.EffectiveDate);
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id, companyId = request.CompanyId }, result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetExchangeRateByIdQuery(companyId, id), cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] Guid companyId, [FromQuery] string? fromCurrencyCode = null, [FromQuery] string? toCurrencyCode = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListExchangeRatesQuery(companyId, fromCurrencyCode, toCurrencyCode, page, pageSize), cancellationToken);
        return Ok(result);
    }

    // Read-only utility endpoint, not a resource under this collection — kept
    // on this controller (rather than a separate one) since it's purely a
    // query over ExchangeRate data, same "utility action alongside CRUD"
    // placement as BankStatementLinesController's GetSummary.
    [HttpGet("convert")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Convert([FromQuery] Guid companyId, [FromQuery] string from, [FromQuery] string to, [FromQuery] decimal amount, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ConvertAmountQuery(companyId, from, to, amount), cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateExchangeRateRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateExchangeRateCommand(request.CompanyId, id, request.Rate, request.EffectiveDate);
        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }

    // Soft-deactivate only — never a real delete (04_DATABASE_GUIDELINES.md), same
    // body-bound-request convention as CostCentersController.Deactivate.
    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(Guid id, [FromBody] DeactivateExchangeRateRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeactivateExchangeRateCommand(request.CompanyId, id), cancellationToken);
        return Ok(result);
    }
}

public sealed record CreateExchangeRateRequest(Guid CompanyId, string FromCurrencyCode, string ToCurrencyCode, decimal Rate, DateTimeOffset EffectiveDate);

public sealed record UpdateExchangeRateRequest(Guid CompanyId, decimal Rate, DateTimeOffset EffectiveDate);

public sealed record DeactivateExchangeRateRequest(Guid CompanyId);
