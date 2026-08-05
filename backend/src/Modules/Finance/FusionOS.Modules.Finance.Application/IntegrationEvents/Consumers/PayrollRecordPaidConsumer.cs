using System.Text.Json;
using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.AccountingPeriods.Contracts;
using FusionOS.Modules.Finance.Application.JournalEntries.Contracts;
using FusionOS.Modules.Finance.Application.Settings.Contracts;
using FusionOS.Modules.Finance.Domain.JournalEntries;
using FusionOS.SharedKernel.Events;

namespace FusionOS.Modules.Finance.Application.IntegrationEvents.Consumers;

/// <summary>
/// Reacts to HRMS's PayrollRecordPaid domain event (relayed via the outbox to
/// Kafka — 03_SYSTEM_ARCHITECTURE.md §4.2) — the GL-posting integration
/// PayrollRecord.MarkPaid's own doc comment used to flag as "a distinct,
/// separately-scoped future slice." Defines its own local payload shape
/// rather than referencing Hrms's domain event CLR type, same reviewed
/// pattern as every other consumer in this codebase (InvoiceIssuedConsumer,
/// PurchaseOrderGoodsReceiptCostedConsumer): a consumer must never take a
/// compile-time dependency on the producing module's internals.
///
/// Posts a balanced Debit Salary Expense / Credit Salary Payable JournalEntry
/// for GrossPay when FinanceSettings has both
/// <see cref="Domain.Settings.FinanceSettings.DefaultSalaryExpenseAccountId"/>
/// and <see cref="Domain.Settings.FinanceSettings.DefaultSalaryPayableAccountId"/>
/// configured. When FinanceSettings is unconfigured (either account still
/// null), this silently skips the GL post — same restraint as
/// InvoiceIssuedConsumer/PurchaseOrderGoodsReceiptCostedConsumer: a Kafka
/// consumer has no user-facing channel to surface a "please configure
/// Finance settings" validation error the way a command handler does, so
/// silently skipping is the only sound option here, not a shortcut. Unlike
/// those two consumers, there is no subledger to update either way — Payroll
/// has no AR/AP-style ledger of its own in this slice, so when settings are
/// unconfigured this consumer has nothing left to do but mark the event
/// processed.
///
/// Phase 10/11 hardening fix (2026-07-27): also skips the GL post — same
/// silent-skip reasoning as the unconfigured-settings case above, since a
/// background consumer has no user-facing channel to surface a validation
/// error — when the payroll-paid date falls inside an already-closed
/// accounting period, so this never trips PostJournalEntryCommandHandler's
/// closed-period guard by creating an already-Posted entry that bypasses it.
/// </summary>
public sealed class PayrollRecordPaidConsumer : IIntegrationEventConsumer
{
    private readonly IJournalEntryRepository _journalEntryRepository;
    private readonly IAccountingPeriodRepository _accountingPeriodRepository;
    private readonly IFinanceSettingsRepository _settingsRepository;
    private readonly IProcessedIntegrationEventStore _processedEvents;
    private readonly IUnitOfWork _unitOfWork;

    public PayrollRecordPaidConsumer(
        IJournalEntryRepository journalEntryRepository,
        IAccountingPeriodRepository accountingPeriodRepository,
        IFinanceSettingsRepository settingsRepository,
        IProcessedIntegrationEventStore processedEvents,
        IUnitOfWork unitOfWork)
    {
        _journalEntryRepository = journalEntryRepository;
        _accountingPeriodRepository = accountingPeriodRepository;
        _settingsRepository = settingsRepository;
        _processedEvents = processedEvents;
        _unitOfWork = unitOfWork;
    }

    public string EventType => "PayrollRecordPaid";

    public async Task HandleAsync(Guid eventId, Guid companyId, string payloadJson, CancellationToken cancellationToken)
    {
        if (await _processedEvents.HasProcessedAsync(eventId, cancellationToken))
        {
            return; // already applied — at-least-once redelivery, this is the dedupe guard.
        }

        var payload = JsonSerializer.Deserialize<Payload>(payloadJson)
            ?? throw new InvalidOperationException($"Could not deserialize PayrollRecordPaid payload for event {eventId}.");

        var settings = await _settingsRepository.GetByCompanyIdAsync(payload.CompanyId, cancellationToken);
        var payrollPaidDate = new DateTimeOffset(payload.PeriodYear, payload.PeriodMonth, 1, 0, 0, 0, TimeSpan.Zero);
        var closedPeriod = await _accountingPeriodRepository.FindClosedPeriodCoveringDateAsync(payload.CompanyId, payrollPaidDate, cancellationToken);
        if (settings?.DefaultSalaryExpenseAccountId is { } expenseAccountId && settings.DefaultSalaryPayableAccountId is { } payableAccountId && closedPeriod is null)
        {
            var reference = $"Payroll {payload.PayrollRecordId} paid ({payload.PeriodMonth:D2}/{payload.PeriodYear})";
            var lines = new[]
            {
                new JournalEntryLineInput(expenseAccountId, payload.GrossPay, 0m, reference),
                new JournalEntryLineInput(payableAccountId, 0m, payload.GrossPay, reference),
            };
            var journalEntry = JournalEntry.Create(payload.CompanyId, reference, lines);
            journalEntry.Post();
            await _journalEntryRepository.AddAsync(journalEntry, cancellationToken);
        }

        _processedEvents.MarkProcessed(eventId, EventType);

        // One SaveChangesAsync — the optional journal entry and the
        // idempotency marker share the same scoped DbContext instance, so
        // this commits both atomically.
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private sealed record Payload(Guid PayrollRecordId, Guid CompanyId, Guid EmployeeId, decimal GrossPay, int PeriodMonth, int PeriodYear);
}
