using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.Budgets.Contracts;
using FusionOS.Modules.Finance.Application.BudgetLines.Contracts;
using FusionOS.Modules.Finance.Application.JournalEntries.Contracts;
using MediatR;

namespace FusionOS.Modules.Finance.Application.Budgets.Queries.GetBudgetVsActual;

/// <summary>
/// For every BudgetLine on the given Budget, looks up the actual posted-
/// JournalEntry total for that line's AccountId within the Budget's
/// PeriodStart/PeriodEnd (via IJournalEntryRepository.
/// SumPostedAmountByAccountAsync — Posted entries only, Draft entries never
/// affect the ledger, see JournalEntry.cs's own class doc comment) and
/// returns budgeted/actual/variance side by side.
///
/// Cost-center awareness (resolved): JournalEntryLine now carries an optional
/// CostCenterId, so when a BudgetLine specifies a CostCenterId the actual side
/// is restricted to postings tagged with that same cost center (the CostCenterId
/// filter is threaded straight through SumPostedAmountByAccountAsync). When a
/// BudgetLine has no CostCenterId, behaviour is unchanged — account-level across
/// all cost centers. Two BudgetLines on the same Budget referencing the same
/// AccountId with different CostCenterIds now each get their own cost-center-
/// scoped actual, rather than the same account-level total.
/// </summary>
public sealed class GetBudgetVsActualQueryHandler : IRequestHandler<GetBudgetVsActualQuery, IReadOnlyList<BudgetVsActualLineDto>>
{
    private readonly IBudgetRepository _budgetRepository;
    private readonly IBudgetLineRepository _budgetLineRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IJournalEntryRepository _journalEntryRepository;

    public GetBudgetVsActualQueryHandler(
        IBudgetRepository budgetRepository,
        IBudgetLineRepository budgetLineRepository,
        IAccountRepository accountRepository,
        IJournalEntryRepository journalEntryRepository)
    {
        _budgetRepository = budgetRepository;
        _budgetLineRepository = budgetLineRepository;
        _accountRepository = accountRepository;
        _journalEntryRepository = journalEntryRepository;
    }

    public async Task<IReadOnlyList<BudgetVsActualLineDto>> Handle(GetBudgetVsActualQuery request, CancellationToken cancellationToken)
    {
        var budget = await _budgetRepository.GetByIdAsync(request.CompanyId, request.BudgetId, cancellationToken)
            ?? throw new KeyNotFoundException($"Budget '{request.BudgetId}' was not found.");

        var budgetLines = await _budgetLineRepository.ListAllByBudgetAsync(request.CompanyId, request.BudgetId, cancellationToken);

        var results = new List<BudgetVsActualLineDto>();
        foreach (var line in budgetLines)
        {
            var account = await _accountRepository.GetByIdAsync(request.CompanyId, line.AccountId, cancellationToken)
                ?? throw new KeyNotFoundException($"Account '{line.AccountId}' referenced by budget line '{line.Id}' was not found.");

            var actualAmount = await _journalEntryRepository.SumPostedAmountByAccountAsync(
                request.CompanyId, line.AccountId, budget.PeriodStart, budget.PeriodEnd, line.CostCenterId, cancellationToken);

            results.Add(new BudgetVsActualLineDto(
                line.AccountId,
                account.Code,
                account.Name,
                line.CostCenterId,
                line.BudgetedAmount,
                actualAmount,
                actualAmount - line.BudgetedAmount));
        }

        return results;
    }
}
