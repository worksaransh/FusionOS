using FusionOS.SharedKernel;
using FusionOS.Modules.Finance.Domain.JournalEntries.Events;

namespace FusionOS.Modules.Finance.Domain.JournalEntries;

/// <summary>
/// The core General Ledger transaction (05_MODULE_ROADMAP.md Phase 2 — Financial
/// Backbone). A JournalEntry must always balance (total debits == total credits)
/// — this is enforced at creation, not deferred to posting time, since an
/// unbalanced entry is never a valid intermediate state in this model. Draft
/// entries do not affect account balances; only Posted ones do (real-world
/// double-entry accounting semantics) — reporting/balance calculation is a
/// follow-up slice once this posting workflow exists.
/// </summary>
public sealed class JournalEntry : TenantAggregateRoot
{
    private readonly List<JournalEntryLine> _lines = new();

    public DateTimeOffset EntryDate { get; private set; }
    public string? Reference { get; private set; }
    public JournalEntryStatus Status { get; private set; }
    public IReadOnlyList<JournalEntryLine> Lines => _lines.AsReadOnly();
    public decimal TotalDebit => _lines.Sum(l => l.Debit);
    public decimal TotalCredit => _lines.Sum(l => l.Credit);

    private JournalEntry() { }

    public static JournalEntry Create(Guid companyId, string? reference, IReadOnlyCollection<JournalEntryLineInput> lines)
    {
        if (lines is null || lines.Count < 2)
            throw new ArgumentException("A journal entry must have at least two lines.", nameof(lines));

        var entry = new JournalEntry
        {
            CompanyId = companyId,
            Reference = reference?.Trim(),
            Status = JournalEntryStatus.Draft,
            EntryDate = DateTimeOffset.UtcNow,
        };

        foreach (var line in lines)
            entry._lines.Add(JournalEntryLine.Create(line.AccountId, line.Debit, line.Credit, line.Description));

        if (entry.TotalDebit != entry.TotalCredit)
            throw new InvalidOperationException($"Journal entry is not balanced: total debit {entry.TotalDebit} != total credit {entry.TotalCredit}.");

        entry.Raise(new JournalEntryCreated(entry.Id, companyId, entry.TotalDebit));
        return entry;
    }

    /// <summary>Raises JournalEntryPosted — the point this entry actually affects the General Ledger.</summary>
    public void Post()
    {
        if (Status != JournalEntryStatus.Draft)
            throw new InvalidOperationException($"Only a Draft journal entry can be posted (current status: {Status}).");

        Status = JournalEntryStatus.Posted;
        Raise(new JournalEntryPosted(Id, CompanyId, TotalDebit));
    }
}
