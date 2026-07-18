using FusionOS.Modules.Finance.Application.FixedAssets.Commands.CreateFixedAsset;
using FusionOS.Modules.Finance.Application.FixedAssets.Commands.DeactivateFixedAsset;
using FusionOS.Modules.Finance.Application.FixedAssets.Commands.DisposeFixedAsset;
using FusionOS.Modules.Finance.Application.FixedAssets.Commands.PostMonthlyDepreciation;
using FusionOS.Modules.Finance.Application.FixedAssets.Commands.UpdateFixedAsset;
using FusionOS.Modules.Finance.Application.FixedAssets.Queries.GetDepreciationSchedule;
using FusionOS.Modules.Finance.Application.FixedAssets.Queries.GetFixedAssetById;
using FusionOS.Modules.Finance.Application.FixedAssets.Queries.ListFixedAssets;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Finance.Api.Controllers;

/// <summary>
/// M8g — Finance depth: fixed assets. CRUD at the top level, same shape as
/// CostCentersController, plus a `.../{id}/dispose` action (a genuine
/// one-way business state change, distinct from Deactivate — see
/// FixedAsset.Dispose's own doc comment) and a read-only
/// `.../{id}/depreciation-schedule` calculation action wired to
/// GetDepreciationScheduleQuery — a read-only utility action alongside CRUD,
/// same placement precedent as BankStatementLinesController.GetSummary/
/// BudgetsController.GetVsActual.
/// </summary>
[ApiController]
[Route("api/v1/finance/fixed-assets")]
public sealed class FixedAssetsController : ControllerBase
{
    private readonly ISender _sender;

    public FixedAssetsController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateFixedAssetRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateFixedAssetCommand(
            request.CompanyId, request.Code, request.Name, request.AssetAccountId,
            request.AccumulatedDepreciationAccountId, request.CostCenterId,
            request.AcquisitionDate, request.AcquisitionCost, request.SalvageValue, request.UsefulLifeMonths);
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id, companyId = request.CompanyId }, result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetFixedAssetByIdQuery(companyId, id), cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] Guid companyId, [FromQuery] bool? isDisposed = null, [FromQuery] bool? isActive = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListFixedAssetsQuery(companyId, isDisposed, isActive, page, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateFixedAssetRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateFixedAssetCommand(request.CompanyId, id, request.Name, request.CostCenterId);
        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }

    // Soft-deactivate only — never a real delete (04_DATABASE_GUIDELINES.md), same
    // body-bound-request convention as CostCentersController.Deactivate.
    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(Guid id, [FromBody] DeactivateFixedAssetRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeactivateFixedAssetCommand(request.CompanyId, id), cancellationToken);
        return Ok(result);
    }

    // Records the asset as disposed — no gain/loss calculation or GL posting
    // (see FixedAsset.Dispose's own doc comment).
    [HttpPost("{id:guid}/dispose")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Dispose(Guid id, [FromBody] DisposeFixedAssetRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DisposeFixedAssetCommand(request.CompanyId, id, request.DisposedDate), cancellationToken);
        return Ok(result);
    }

    // Posts one month of straight-line depreciation as a real (Posted) JournalEntry
    // — Debit the supplied expense account, Credit the asset's accumulated-
    // depreciation account. Unlike the read-only schedule below, this DOES write to
    // the GL. See PostMonthlyDepreciationCommand's own doc comment for scope.
    [HttpPost("{id:guid}/post-depreciation")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PostDepreciation(Guid id, [FromBody] PostDepreciationRequest request, CancellationToken cancellationToken)
    {
        var command = new PostMonthlyDepreciationCommand(request.CompanyId, id, request.DepreciationExpenseAccountId, request.PeriodEnd);
        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }

    // Read-only calculation — see GetDepreciationScheduleQueryHandler's own
    // doc comment: nothing here is persisted, no JournalEntry is ever created.
    [HttpGet("{id:guid}/depreciation-schedule")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDepreciationSchedule(Guid id, [FromQuery] Guid companyId, [FromQuery] DateTimeOffset asOfDate, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetDepreciationScheduleQuery(companyId, id, asOfDate), cancellationToken);
        return Ok(result);
    }
}

public sealed record CreateFixedAssetRequest(
    Guid CompanyId,
    string Code,
    string Name,
    Guid AssetAccountId,
    Guid? AccumulatedDepreciationAccountId,
    Guid? CostCenterId,
    DateTimeOffset AcquisitionDate,
    decimal AcquisitionCost,
    decimal SalvageValue,
    int UsefulLifeMonths);

public sealed record UpdateFixedAssetRequest(Guid CompanyId, string Name, Guid? CostCenterId);

public sealed record DeactivateFixedAssetRequest(Guid CompanyId);

public sealed record DisposeFixedAssetRequest(Guid CompanyId, DateTimeOffset DisposedDate);

public sealed record PostDepreciationRequest(Guid CompanyId, Guid DepreciationExpenseAccountId, DateTimeOffset PeriodEnd);
