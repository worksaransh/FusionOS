using FusionOS.Modules.Finance.Application.Budgets.Commands.CreateBudget;
using FusionOS.Modules.Finance.Application.Budgets.Commands.DeactivateBudget;
using FusionOS.Modules.Finance.Application.Budgets.Commands.UpdateBudget;
using FusionOS.Modules.Finance.Application.Budgets.Queries.GetBudgetById;
using FusionOS.Modules.Finance.Application.Budgets.Queries.GetBudgetVsActual;
using FusionOS.Modules.Finance.Application.Budgets.Queries.ListBudgets;
using FusionOS.Modules.Finance.Application.BudgetLines.Commands.CreateBudgetLine;
using FusionOS.Modules.Finance.Application.BudgetLines.Commands.UpdateBudgetLineAmount;
using FusionOS.Modules.Finance.Application.BudgetLines.Queries.ListBudgetLines;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Finance.Api.Controllers;

/// <summary>
/// M8f — Finance depth: budgeting. Budget CRUD at the top level, same shape
/// as CostCentersController, plus BudgetLine CRUD nested under
/// `.../budgets/{budgetId}/lines` (same nesting convention
/// BankStatementLinesController uses for lines under a bank account) and a
/// read-only `GET .../budgets/{budgetId}/vs-actual` report action wired to
/// GetBudgetVsActualQuery — a read-only utility action alongside CRUD, same
/// placement precedent as BankStatementLinesController.GetSummary/
/// ExchangeRatesController.Convert.
/// </summary>
[ApiController]
[Route("api/v1/finance/budgets")]
public sealed class BudgetsController : ControllerBase
{
    private readonly ISender _sender;

    public BudgetsController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateBudgetRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateBudgetCommand(request.CompanyId, request.Name, request.PeriodStart, request.PeriodEnd);
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id, companyId = request.CompanyId }, result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetBudgetByIdQuery(companyId, id), cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] Guid companyId, [FromQuery] string? search = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListBudgetsQuery(companyId, search, page, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBudgetRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateBudgetCommand(request.CompanyId, id, request.Name, request.PeriodStart, request.PeriodEnd);
        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }

    // Soft-deactivate only — never a real delete (04_DATABASE_GUIDELINES.md), same
    // body-bound-request convention as CostCentersController.Deactivate.
    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(Guid id, [FromBody] DeactivateBudgetRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeactivateBudgetCommand(request.CompanyId, id), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{budgetId:guid}/vs-actual")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetVsActual(Guid budgetId, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetBudgetVsActualQuery(companyId, budgetId), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{budgetId:guid}/lines")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateLine(Guid budgetId, [FromBody] CreateBudgetLineRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateBudgetLineCommand(request.CompanyId, budgetId, request.AccountId, request.CostCenterId, request.BudgetedAmount, request.Notes);
        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{budgetId:guid}/lines")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ListLines(Guid budgetId, [FromQuery] Guid companyId, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListBudgetLinesQuery(companyId, budgetId, page, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpPut("{budgetId:guid}/lines/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateLine(Guid budgetId, Guid id, [FromBody] UpdateBudgetLineAmountRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateBudgetLineAmountCommand(request.CompanyId, id, request.BudgetedAmount, request.Notes);
        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }
}

public sealed record CreateBudgetRequest(Guid CompanyId, string Name, DateTimeOffset PeriodStart, DateTimeOffset PeriodEnd);

public sealed record UpdateBudgetRequest(Guid CompanyId, string Name, DateTimeOffset PeriodStart, DateTimeOffset PeriodEnd);

public sealed record DeactivateBudgetRequest(Guid CompanyId);

public sealed record CreateBudgetLineRequest(Guid CompanyId, Guid AccountId, Guid? CostCenterId, decimal BudgetedAmount, string? Notes);

public sealed record UpdateBudgetLineAmountRequest(Guid CompanyId, decimal BudgetedAmount, string? Notes);
