using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Core.Application.Companies.Contracts;
using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.IntercompanyEliminationRules.Contracts;
using FusionOS.Modules.Finance.Application.JournalEntries.Contracts;
using FusionOS.Modules.Finance.Application.Reports.Contracts;
using FusionOS.SharedKernel.Context;
using MediatR;

namespace FusionOS.Modules.Finance.Application.Reports.Queries.GetConsolidatedTrialBalance;

/// <summary>
/// Group-level rollup over GetTrialBalanceQueryHandler's own per-company logic
/// (same GetPostedBalancesByAccountAsOfAsync + chart-of-accounts join), summed
/// by account CODE across every entity in the group instead of returning one
/// company's AccountId-keyed rows, then netted against active
/// IntercompanyEliminationRule pairs so the same intercompany transaction isn't
/// double-counted as both an asset and a liability at the consolidated level.
/// </summary>
public sealed class GetConsolidatedTrialBalanceQueryHandler : IRequestHandler<GetConsolidatedTrialBalanceQuery, ConsolidatedTrialBalanceReportDto>
{
    private readonly ICompanyRepository _companyRepository;
    private readonly IJournalEntryRepository _journalEntryRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IIntercompanyEliminationRuleRepository _eliminationRuleRepository;
    private readonly ICurrentUserContext _currentUser;

    public GetConsolidatedTrialBalanceQueryHandler(
        ICompanyRepository companyRepository,
        IJournalEntryRepository journalEntryRepository,
        IAccountRepository accountRepository,
        IIntercompanyEliminationRuleRepository eliminationRuleRepository,
        ICurrentUserContext currentUser)
    {
        _companyRepository = companyRepository;
        _journalEntryRepository = journalEntryRepository;
        _accountRepository = accountRepository;
        _eliminationRuleRepository = eliminationRuleRepository;
        _currentUser = currentUser;
    }

    public async Task<ConsolidatedTrialBalanceReportDto> Handle(GetConsolidatedTrialBalanceQuery request, CancellationToken cancellationToken)
    {
        // This query carries ParentCompanyId, not CompanyId, so
        // TenantIsolationBehavior's reflective CompanyId check can't apply here
        // (same situation as GetCompanyByIdQuery/UpdateCompanyCommand) — enforce
        // the equivalent restriction directly: only the parent company's own
        // caller may consolidate it.
        if (_currentUser.CompanyId is not { } callerCompanyId || callerCompanyId != request.ParentCompanyId)
            throw new ForbiddenException($"company-scope:{request.ParentCompanyId}");

        var parent = await _companyRepository.GetByIdAsync(request.ParentCompanyId, cancellationToken)
            ?? throw new KeyNotFoundException($"Company '{request.ParentCompanyId}' was not found.");

        var subsidiaries = await _companyRepository.ListByParentCompanyIdAsync(request.ParentCompanyId, cancellationToken);

        var companyIds = new List<Guid> { parent.Id };
        companyIds.AddRange(subsidiaries.Select(s => s.Id));

        // Sum every rolled-up entity's posted balances by account CODE (not
        // AccountId — each entity has its own chart-of-accounts Account rows;
        // the code is the only thing shared across every entity's chart).
        var totalsByCode = new Dictionary<string, (decimal Debit, decimal Credit)>(StringComparer.Ordinal);

        foreach (var companyId in companyIds)
        {
            var balances = await _journalEntryRepository.GetPostedBalancesByAccountAsOfAsync(companyId, request.AsOf, cancellationToken);
            if (balances.Count == 0)
                continue;

            var accounts = (await _accountRepository.ListAllAsync(companyId, cancellationToken)).ToDictionary(a => a.Id);

            foreach (var b in balances)
            {
                if (!accounts.TryGetValue(b.AccountId, out var account))
                    throw new KeyNotFoundException($"Account '{b.AccountId}' referenced by a posted journal entry was not found.");

                var hasExisting = totalsByCode.TryGetValue(account.Code, out var current);
                var existingDebit = hasExisting ? current.Debit : 0m;
                var existingCredit = hasExisting ? current.Credit : 0m;
                totalsByCode[account.Code] = (existingDebit + b.TotalDebit, existingCredit + b.TotalCredit);
            }
        }

        var adjustedCodes = new HashSet<string>(StringComparer.Ordinal);

        // Net out active intercompany elimination rules owned by the parent —
        // reduce both sides of a receivable/payable pair by whatever amount
        // they actually have in common, so a partial/mismatched intercompany
        // balance doesn't get either over- or under-eliminated.
        var rules = await _eliminationRuleRepository.ListActiveAsync(parent.Id, cancellationToken);
        foreach (var rule in rules)
        {
            if (!totalsByCode.TryGetValue(rule.ReceivableAccountCode, out var receivable) ||
                !totalsByCode.TryGetValue(rule.PayableAccountCode, out var payable))
                continue;

            var receivableNet = receivable.Debit - receivable.Credit;
            var payableNet = payable.Credit - payable.Debit;
            var eliminationAmount = Math.Min(Math.Max(receivableNet, 0m), Math.Max(payableNet, 0m));
            if (eliminationAmount <= 0m)
                continue;

            totalsByCode[rule.ReceivableAccountCode] = (receivable.Debit - eliminationAmount, receivable.Credit);
            totalsByCode[rule.PayableAccountCode] = (payable.Debit, payable.Credit - eliminationAmount);
            adjustedCodes.Add(rule.ReceivableAccountCode);
            adjustedCodes.Add(rule.PayableAccountCode);
        }

        var lines = totalsByCode
            .Select(kv => new ConsolidatedTrialBalanceLineDto(
                kv.Key,
                kv.Value.Debit,
                kv.Value.Credit,
                kv.Value.Debit - kv.Value.Credit,
                adjustedCodes.Contains(kv.Key)))
            .OrderBy(l => l.AccountCode, StringComparer.Ordinal)
            .ToList();

        var totalDebit = lines.Sum(l => l.TotalDebit);
        var totalCredit = lines.Sum(l => l.TotalCredit);

        return new ConsolidatedTrialBalanceReportDto(request.AsOf, parent.Id, companyIds, lines, totalDebit, totalCredit, totalDebit == totalCredit);
    }
}
